#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using dnlib.DotNet;
using dnlib.PE;
using NativeSharp;

namespace ExtremeDumper.Dumping {
	internal sealed unsafe class NormalDumper : IDumper {
		private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

		private readonly NativeProcess _process;

		private NormalDumper(uint processId) {
			_process = NativeProcess.Open(processId, ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
		}

		public static IDumper Create(uint processId) {
			return new NormalDumper(processId);
		}

		[HandleProcessCorruptedStateExceptions]
		public bool DumpModule(IntPtr moduleHandle, ImageLayout imageLayout, string filePath) {
			try {
				byte[] peImage = PEImageDumper.Dump(_process, (void*)moduleHandle, ref imageLayout);
				if (peImage is null)
					return false;

				peImage = PEImageDumper.ConvertImageLayout(peImage, imageLayout, ImageLayout.File);
				File.WriteAllBytes(filePath, peImage);
				return true;
			}
			catch {
				return false;
			}
		}

		public int DumpProcess(string directoryPath) {
			int count = 0;
			var originalFileCache = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
			foreach (var pageInfo in _process.EnumeratePageInfos()) {
				if (!IsValidPage(pageInfo))
					continue;
				byte[] page = new byte[Math.Min((int)pageInfo.Size, 0x2000)];
				if (!_process.TryReadBytes(pageInfo.Address, page))
					continue;

				for (int i = 0; i < page.Length - 0x200; i++) {
					fixed (byte* p = page) {
						if (!MaybePEImage(p + i, page.Length - i))
							continue;
					}

					var imageLayout = i == 0 ? GetProbableImageLayout(page) : ImageLayout.File;
					byte[] peImage = DumpDotNetModule(_process, (byte*)pageInfo.Address + i, imageLayout, out string fileName);
					if (peImage is null && i == 0) {
						// 也许判断有误，尝试一下另一种格式
						if (imageLayout == ImageLayout.Memory)
							peImage = DumpDotNetModule(_process, (byte*)pageInfo.Address + i, ImageLayout.File, out fileName);
						else
							peImage = DumpDotNetModule(_process, (byte*)pageInfo.Address + i, ImageLayout.Memory, out fileName);
					}

					if (peImage is null)
						continue;

					if (BuiltInAssemblyHelper.IsBuiltInAssembly(peImage))
						continue;

					fileName = EnsureValidFileName(fileName);
					if (!IsSameFile(directoryPath, fileName, peImage, originalFileCache)) {
						string filePath = Path.Combine(directoryPath, EnsureNoRepeatFileName(directoryPath, fileName));
						File.WriteAllBytes(filePath, peImage);
					}
					count++;
				}
			}
			return count;
		}

		private static bool IsValidPage(PageInfo pageInfo) {
			return pageInfo.Protection != 0 && (pageInfo.Protection & MemoryProtection.NoAccess) == 0 && (ulong)pageInfo.Size <= int.MaxValue;
		}

		[HandleProcessCorruptedStateExceptions]
		private static bool MaybePEImage(byte* p, int size) {
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
		private static ImageLayout GetProbableImageLayout(byte[] firstPage) {
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
		private static byte[] DumpDotNetModule(NativeProcess process, void* address, ImageLayout imageLayout, out string fileName) {
			fileName = default;
			try {
				byte[] data = PEImageDumper.Dump(process, address, ref imageLayout);
				if (data is null)
					return null;

				data = PEImageDumper.ConvertImageLayout(data, imageLayout, ImageLayout.File);
				bool isDotNet;
				using var peImage = new PEImage(data, true);
				// 确保为有效PE文件
				isDotNet = peImage.ImageNTHeaders.OptionalHeader.DataDirectories[14].VirtualAddress != 0;
				if (isDotNet) {
					try {
						using var moduleDef = ModuleDefMD.Load(peImage);
						// 再次验证是否为.NET程序集
						if (string.IsNullOrEmpty(fileName))
							fileName = moduleDef.Assembly.Name + (moduleDef.EntryPoint is null ? ".dll" : ".exe");
					}
					catch {
						isDotNet = false;
					}
				}
				if (string.IsNullOrEmpty(fileName))
					fileName = ((IntPtr)address).ToString((ulong)address > uint.MaxValue ? "X16" : "X8");
				return isDotNet ? data : null;
			}
			catch {
				return null;
			}
		}

		private static string EnsureValidFileName(string fileName) {
			if (string.IsNullOrEmpty(fileName))
				return string.Empty;

			var newFileName = new StringBuilder(fileName.Length);
			foreach (char chr in fileName) {
				if (!InvalidFileNameChars.Contains(chr))
					newFileName.Append(chr);
			}
			return newFileName.ToString();
		}

		private static bool IsSameFile(string directoryPath, string fileName, byte[] data, Dictionary<string, byte[]> originalFileCache) {
			string filePath = Path.Combine(directoryPath, fileName);
			if (!File.Exists(filePath)) {
				originalFileCache[fileName] = data;
				return false;
			}

			if (!originalFileCache.TryGetValue(fileName, out byte[] originalData)) {
				originalData = File.ReadAllBytes(filePath);
				originalFileCache.Add(fileName, originalData);
			}

			if (data.Length != originalData.Length)
				return false;

			for (int i = 0; i < data.Length; i++) {
				if (data[i] != originalData[i])
					return false;
			}

			return true;
		}

		private static string EnsureNoRepeatFileName(string directoryPath, string fileName) {
			int count = 1;
			string fileNameWithoutExtension = null;
			string extension = null;
			string filePath;
			while (File.Exists(filePath = Path.Combine(directoryPath, fileName))) {
				if (fileNameWithoutExtension is null) {
					fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
					extension = Path.GetExtension(fileName);
				}
				count++;
				fileName = $"{fileNameWithoutExtension} ({count}){extension}";
			}
			return filePath;
		}

		public void Dispose() {
			_process.Dispose();
		}
	}
}
