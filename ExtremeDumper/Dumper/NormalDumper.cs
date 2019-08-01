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

		private readonly uint _processId;

		private NormalDumper(uint processId) {
			_processId = processId;
		}

		public static IDumper Create(uint processId) {
			return new NormalDumper(processId);
		}

		public bool DumpModule(IntPtr moduleHandle, ImageLayout imageLayout, string filePath) {
			try {
				byte[] peImageData;

				peImageData = PEImageHelper.DirectCopy(_processId, (void*)moduleHandle, imageLayout);
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
			using (NativeProcess process = NativeProcess.Open(_processId, ProcessAccess.MemoryOperation | ProcessAccess.MemoryRead | ProcessAccess.QueryInformation)) {
				bool is64Bit;

				is64Bit = process.Is64Bit;
				foreach (PageInfo pageInfo in process.EnumeratePageInfos()) {
					ushort magic;
					byte[] peHeaderData;
					byte[] peImageData;
					string fileName;
					bool isDotNet;
					string filePath;

					if ((ulong)pageInfo.Size > int.MaxValue)
						continue;
					if (!process.TryReadUInt16(pageInfo.Address, out magic))
						continue;
					if (magic != 0x5A4D)
						// MZ
						continue;
					peHeaderData = new byte[(uint)pageInfo.Size];
					if (!process.TryReadBytes(pageInfo.Address, peHeaderData))
						continue;
					try {
						uint imageSize;
						ImageLayout imageLayout;

						imageSize = PEImageHelper.GetImageSize(peHeaderData, ImageLayout.File);
						// 获取文件格式大小
						imageLayout = imageSize >= (uint)pageInfo.Size ? ImageLayout.Memory : ImageLayout.File;
						// 如果文件格式大小大于页面大小，说明在内存中是内存格式的，反之为文件格式
						peImageData = PEImageHelper.DirectCopy(_processId, pageInfo.Address, imageLayout);
						peImageData = PEImageHelper.ConvertImageLayout(peImageData, imageLayout, ImageLayout.File);
						using (PEImage peImage = new PEImage(peImageData, true)) {
							// 确保为有效PE文件
							fileName = peImage.GetOriginalFilename() ?? ((IntPtr)pageInfo.Address).ToString(is64Bit ? "X16" : "X8");
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
					}
					catch {
						continue;
					}
					if (!isDotNet)
						continue;
					filePath = Path.Combine(directoryPath, EnsureNoRepeatFileName(directoryPath, EnsureValidFileName(fileName)));
					File.WriteAllBytes(filePath, peImageData);
					count++;
				}
			}
			return count;
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
		}
	}
}
