using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.PE;
using ExtremeDumper.Logging;
using NativeSharp;

namespace ExtremeDumper.Dumping;

sealed unsafe class NormalDumper : DumperBase {
	public NormalDumper(uint processId) : base(processId) {
	}

	[HandleProcessCorruptedStateExceptions]
	public override bool DumpModule(nuint moduleHandle, ImageLayout imageLayout, string filePath) {
		try {
			var peImage = PEImageDumper.Dump(process, moduleHandle, ref imageLayout);
			if (peImage is null)
				return false;

			peImage = PEImageDumper.ConvertImageLayout(peImage, imageLayout, ImageLayout.File);
			File.WriteAllBytes(filePath, peImage);
			return true;
		}
		catch (Exception ex) {
			Logger.Exception(ex);
			return false;
		}
	}

	public override int DumpProcess(string directoryPath) {
		int count = 0;
		var originalFileCache = new ConcurrentDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
		Parallel.ForEach(process.EnumeratePageInfos(), pageInfo => {
			if (!IsValidPage(pageInfo))
				return;
			var page = new byte[Math.Min((int)pageInfo.Size, 0x40000000)];
			// 0x40000000 bytes = 1 giga bytes
			if (!process.TryReadBytes(pageInfo.Address, page))
				return;

			for (int i = 0; i < page.Length - 0x200; i++) {
				fixed (byte* p = page) {
					if (!MaybePEImage(p + i, page.Length - i))
						continue;
				}

				var imageLayout = i == 0 ? GetProbableImageLayout(page) : ImageLayout.File;
				nuint address = (nuint)pageInfo.Address + (uint)i;
				var peImage = DumpDotNetModule(process, address, imageLayout, out var fileName);
				if (peImage is null && i == 0) {
					// 也许判断有误，尝试一下另一种格式。如果不是页面起始位置，必须是文件布局。
					imageLayout = imageLayout == ImageLayout.File ? ImageLayout.Memory : ImageLayout.File;
					peImage = DumpDotNetModule(process, address, ImageLayout.File, out fileName);
				}

				if (peImage is null)
					continue;

				try {
					if (BuiltInAssemblyHelper.IsBuiltInAssembly(peImage))
						continue;
				}
				catch {
					continue;
				}
				var showName = fileName;
				if (SanitizeNames) {
					fileName = Regex.Replace(fileName, @"[^a-zA-Z0-9_\-. ]+", string.Empty, RegexOptions.Singleline).Trim();
					if (String.IsNullOrWhiteSpace(fileName))
						fileName = "unknown";
					if (fileName.StartsWith("."))
						fileName = "unknown" + fileName;
				}
				fileName = EnsureValidFileName(fileName);
				try {
					if (IsSameFile(directoryPath, fileName, peImage, originalFileCache))
						continue;

					fileName = EnsureNoRepeatFileName(directoryPath, fileName);
					var filePath = Path.Combine(directoryPath, fileName);
					File.WriteAllBytes(filePath, peImage);
					count++;
				}
				finally {
					if (showName != fileName)
						showName += $"=>{fileName}";
					Logger.Info($"Found assembly '{showName}' at {Formatter.FormatHex(address)} and image layout is {imageLayout}");
				}
			}
		});
		GC.Collect();
		return count;
	}

	static bool IsValidPage(PageInfo pageInfo) {
		return pageInfo.Protection != 0 && (pageInfo.Protection & MemoryProtection.NoAccess) == 0 && (ulong)pageInfo.Size <= int.MaxValue;
	}

	[HandleProcessCorruptedStateExceptions]
	static bool MaybePEImage(byte* p, int size) {
		try {
			byte* pEnd = p + size;

			if (*(ushort*)p != 0x5A4D)
				return false;

			ushort ntHeadersOffset = *(ushort*)(p + 0x3C);
			p += ntHeadersOffset;
			if (p > pEnd - 4)
				return false;
			if (*(uint*)p != 0x00004550)
				return false;
			p += 0x04;
			// NT headers Signature

			if (p + 0x10 > pEnd - 2)
				return false;
			if (*(ushort*)(p + 0x10) == 0)
				return false;
			p += 0x14;
			// File header SizeOfOptionalHeader

			if (p > pEnd - 2)
				return false;
			if (*(ushort*)p != 0x010B && *(ushort*)p != 0x020B)
				return false;
			// Optional header Magic

			return true;
		}
		catch {
			return false;
		}
	}

	[HandleProcessCorruptedStateExceptions]
	static ImageLayout GetProbableImageLayout(byte[] firstPage) {
		try {
			uint imageSize = PEImageDumper.GetImageSize(firstPage, ImageLayout.File);
			// 获取文件格式大小
			var imageLayout = imageSize >= (uint)firstPage.Length ? ImageLayout.Memory : ImageLayout.File;
			// 如果文件格式大小大于页面大小，说明在内存中是内存格式的，反之为文件格式
			// 这种判断不准确，如果文件文件大小小于最小页面大小，判断会出错
			return imageLayout;
		}
		catch {
			return ImageLayout.Memory;
		}
	}

	[HandleProcessCorruptedStateExceptions]
	static byte[]? DumpDotNetModule(NativeProcess process, nuint address, ImageLayout imageLayout, out string fileName) {
		fileName = string.Empty;
		try {
			var data = PEImageDumper.Dump(process, address, ref imageLayout);
			if (data is null)
				return null;

			data = PEImageDumper.ConvertImageLayout(data, imageLayout, ImageLayout.File);
			using var peImage = new PEImage(data, true);
			// 确保为有效PE文件
			if (peImage.ImageNTHeaders.OptionalHeader.DataDirectories[14].VirtualAddress == 0)
				return null;
			try {
				using var moduleDef = ModuleDefMD.Load(peImage);
				// 再次验证是否为.NET程序集
				if (moduleDef is null)
					return null;
				if (moduleDef.Assembly is not null ? moduleDef.Assembly.Name.Length == 0 : moduleDef.Name.Length == 0)
					return null;
				if (string.IsNullOrEmpty(fileName))
					fileName = moduleDef.Assembly is not null ? (moduleDef.Assembly.Name + (moduleDef.EntryPoint is null ? ".dll" : ".exe")) : moduleDef.Name;
			}
			catch {
				return null;
			}
			if (string.IsNullOrEmpty(fileName))
				fileName = Formatter.FormatHex(address);
			return data;
		}
		catch {
			return null;
		}
	}
}
