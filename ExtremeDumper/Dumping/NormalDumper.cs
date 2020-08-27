using System;
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
			foreach (var pageInfo in _process.EnumeratePageInfos()) {
				if ((ulong)pageInfo.Size > int.MaxValue)
					continue;
				byte[] page = new byte[(int)pageInfo.Size];
				if (!_process.TryReadBytes(pageInfo.Address, page))
					continue;

				for (int i = 0; i < page.Length; i++) {
					fixed (byte* p = page) {
						if (!MaybePEImage(p + i))
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

					string filePath = Path.Combine(directoryPath, EnsureNoRepeatFileName(directoryPath, EnsureValidFileName(fileName)));
					File.WriteAllBytes(filePath, peImage);
					count++;
				}
			}
			return count;
		}

		[HandleProcessCorruptedStateExceptions]
		private static bool MaybePEImage(byte* p) {
			try {
				if (*(ushort*)p != 0x5A4D)
					return false;
				ushort ntHeadersOffset = *(ushort*)(p + 0x3C);
				p += ntHeadersOffset;
				return *(uint*)p == 0x00004550;
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
			try {
				byte[] data = PEImageDumper.Dump(process, address, ref imageLayout);
				data = PEImageDumper.ConvertImageLayout(data, imageLayout, ImageLayout.File);
				bool isDotNet;
				using (var peImage = new PEImage(data, true)) {
					// 确保为有效PE文件
					fileName = peImage.GetOriginalFilename() ?? ((IntPtr)address).ToString((ulong)address > uint.MaxValue ? "X16" : "X8");
					isDotNet = peImage.ImageNTHeaders.OptionalHeader.DataDirectories[14].VirtualAddress != 0;
					if (isDotNet) {
						try {
							using (var moduleDef = ModuleDefMD.Load(peImage)) {
							}
							// 再次验证是否为.NET程序集
						}
						catch {
							isDotNet = false;
						}
					}
				}
				return isDotNet ? data : null;
			}
			catch {
				fileName = default;
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
