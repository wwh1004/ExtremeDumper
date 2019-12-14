using System;
using System.IO;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.PE;
using NativeSharp;

namespace ExtremeDumper.Dumper {
	internal sealed unsafe class NormalDumper : IDumper {
		private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

		private readonly NativeProcess _process;

		private NormalDumper(uint processId) {
			_process = NativeProcess.Open(processId, ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
		}

		public static IDumper Create(uint processId) {
			return new NormalDumper(processId);
		}

		public bool DumpModule(IntPtr moduleHandle, ImageLayout imageLayout, string filePath) {
			try {
				byte[] peImageData;

				peImageData = PEImageHelper.DirectCopy(_process.UnsafeGetModule((void*)moduleHandle), imageLayout, false);
				peImageData = PEImageHelper.ConvertImageLayout(peImageData, imageLayout, ImageLayout.File);
				File.WriteAllBytes(filePath, peImageData);
				return true;
			}
			catch {
				return false;
			}
		}

		public int DumpProcess(string directoryPath) {
			int count;

			count = 0;
			foreach (PageInfo pageInfo in _process.EnumeratePageInfos()) {
				ushort magic;
				byte[] peHeaderData;
				NativeModule module;
				ImageLayout imageLayout;
				byte[] peImageData;
				string fileName;
				string filePath;

				if ((ulong)pageInfo.Size > int.MaxValue)
					continue;
				if (!_process.TryReadUInt16(pageInfo.Address, out magic))
					continue;
				if (magic != 0x5A4D)
					// MZ
					continue;
				peHeaderData = new byte[(uint)pageInfo.Size];
				if (!_process.TryReadBytes(pageInfo.Address, peHeaderData))
					continue;
				module = _process.UnsafeGetModule(pageInfo.Address);
				imageLayout = GetProbableImageLayout(peHeaderData);
				peImageData = DumpDotNetModule(module, imageLayout, out fileName);
				if (peImageData is null) {
					// 也许判断有误，尝试一下另一种格式
					if (imageLayout == ImageLayout.Memory)
						peImageData = DumpDotNetModule(module, ImageLayout.File, out fileName);
					else
						peImageData = DumpDotNetModule(module, ImageLayout.Memory, out fileName);
				}
				if (peImageData is null)
					continue;
				filePath = Path.Combine(directoryPath, EnsureNoRepeatFileName(directoryPath, EnsureValidFileName(fileName)));
				File.WriteAllBytes(filePath, peImageData);
				count++;
			}
			return count;
		}

		private static ImageLayout GetProbableImageLayout(byte[] firstPageData) {
			try {
				uint imageSize;
				ImageLayout imageLayout;

				imageSize = PEImageHelper.GetImageSize(firstPageData, ImageLayout.File);
				// 获取文件格式大小
				imageLayout = imageSize >= (uint)firstPageData.Length ? ImageLayout.Memory : ImageLayout.File;
				// 如果文件格式大小大于页面大小，说明在内存中是内存格式的，反之为文件格式
				// 这种判断不准确，如果文件文件大小小于最小页面大小，判断会出错
				return imageLayout;
			}
			catch {
				return ImageLayout.Memory;
			}
		}

		private static byte[] DumpDotNetModule(NativeModule module, ImageLayout imageLayout, out string fileName) {
			try {
				byte[] peImageData;
				bool isDotNet;

				peImageData = PEImageHelper.DirectCopy(module, imageLayout, false);
				peImageData = PEImageHelper.ConvertImageLayout(peImageData, imageLayout, ImageLayout.File);
				using (PEImage peImage = new PEImage(peImageData, true)) {
					// 确保为有效PE文件
					fileName = peImage.GetOriginalFilename() ?? ((IntPtr)module.Handle).ToString((ulong)module.Handle > uint.MaxValue ? "X16" : "X8");
					isDotNet = peImage.ImageNTHeaders.OptionalHeader.DataDirectories[14].VirtualAddress != 0;
					if (isDotNet)
						try {
							using (ModuleDefMD moduleDef = ModuleDefMD.Load(peImage)) {
							}
							// 再次验证是否为.NET程序集
						}
						catch {
							isDotNet = false;
						}
				}
				return isDotNet ? peImageData : null;
			}
			catch {
				fileName = default;
				return null;
			}
		}

		private static string EnsureValidFileName(string fileName) {
			if (string.IsNullOrEmpty(fileName))
				return string.Empty;

			StringBuilder newFileName;

			newFileName = new StringBuilder(fileName.Length);
			foreach (char chr in fileName)
				if (!InvalidFileNameChars.Contains(chr))
					newFileName.Append(chr);
			return newFileName.ToString();
		}

		private static string EnsureNoRepeatFileName(string directoryPath, string fileName) {
			string filePath;
			int count;
			string fileNameWithoutExtension;
			string extension;

			count = 1;
			fileNameWithoutExtension = null;
			extension = null;
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
