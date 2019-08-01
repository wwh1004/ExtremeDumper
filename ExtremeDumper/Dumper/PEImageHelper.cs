using System;
using System.IO;
using System.Linq;
using dnlib.IO;
using dnlib.PE;
using NativeSharp;

namespace ExtremeDumper.Dumper {
	internal static unsafe class PEImageHelper {
		/// <summary>
		/// 直接从内存中复制模块，不执行格式转换操作
		/// </summary>
		/// <param name="module">模块</param>
		/// <param name="imageLayout">模块在内存中的格式</param>
		/// <returns></returns>
		public static byte[] DirectCopy(NativeModule module, ImageLayout imageLayout) {
			return DirectCopy(module, imageLayout, false, null);
		}

		/// <summary>
		/// 直接从内存中复制模块，不执行格式转换操作
		/// </summary>
		/// <param name="module">模块</param>
		/// <param name="imageLayout">模块在内存中的格式</param>
		/// <param name="useSectionHeadersInFile">是否使用文件中的节头</param>
		/// <param name="alternativeToImagePath">如果无法正常获取模块路径，可提供备选模块路径</param>
		/// <returns></returns>
		public static byte[] DirectCopy(NativeModule module, ImageLayout imageLayout, bool useSectionHeadersInFile, string alternativeToImagePath) {
			if (module is null)
				throw new ArgumentNullException(nameof(module));
			if (useSectionHeadersInFile)
				if (string.IsNullOrEmpty(alternativeToImagePath))
					alternativeToImagePath = null;
				else {
					if (!File.Exists(alternativeToImagePath))
						throw new FileNotFoundException(nameof(alternativeToImagePath));
				}

			NativeProcess process;
			PageInfo firstPageInfo;
			string imagePath;
			byte[] peImageData;
			uint imageSize;

			process = module.Process;
			process.QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			firstPageInfo = process.EnumeratePageInfos(module.Handle, module.Handle).First();
			if (useSectionHeadersInFile) {
				imagePath = module.ImagePath;
				if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
					imagePath = alternativeToImagePath;
			}
			else
				imagePath = default;
			// 获取模块路径（如果需要使用文件中的节头）
			if (useSectionHeadersInFile)
				imageSize = GetImageSize(File.ReadAllBytes(imagePath), imageLayout);
			else {
				byte[] peHeaderData;

				peHeaderData = new byte[(uint)firstPageInfo.Size];
				process.ReadBytes(module.Handle, peHeaderData);
				imageSize = GetImageSize(peHeaderData, imageLayout);
			}
			// 获取模块在内存中的大小
			peImageData = new byte[imageSize];
			switch (imageLayout) {
			case ImageLayout.File:
				if (!process.TryReadBytes(firstPageInfo.Address, peImageData, 0, imageSize))
					throw new InvalidOperationException();
				break;
			case ImageLayout.Memory:
				foreach (PageInfo pageInfo in process.EnumeratePageInfos(module.Handle, (byte*)module.Handle + imageSize)) {
					uint offset;

					offset = (uint)((ulong)pageInfo.Address - (ulong)module.Handle);
					process.TryReadBytes(pageInfo.Address, peImageData, offset, (uint)pageInfo.Size);
				}
				break;
			default:
				throw new NotSupportedException();
			}
			// 转储
			if (useSectionHeadersInFile)
				using (PEImage peHeader = new PEImage(imagePath, false)) {
					int startOffset;
					int endOffset;
					byte[] sectionHeadersData;

					startOffset = (int)peHeader.ImageSectionHeaders.First().StartOffset;
					endOffset = (int)peHeader.ImageSectionHeaders.Last().EndOffset;
					sectionHeadersData = peHeader.CreateReader((FileOffset)startOffset).ReadBytes(endOffset - startOffset);
					Buffer.BlockCopy(sectionHeadersData, 0, peImageData, startOffset, endOffset - startOffset);
				}
			// 替换节头（如果需要使用文件中的节头）
			return peImageData;
		}

		public static byte[] ConvertImageLayout(byte[] peImageData, ImageLayout fromImageLayout, ImageLayout toImageLayout) {
			switch (fromImageLayout) {
			case ImageLayout.File:
			case ImageLayout.Memory:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(fromImageLayout));
			}
			switch (toImageLayout) {
			case ImageLayout.File:
			case ImageLayout.Memory:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(toImageLayout));
			}
			if (peImageData is null)
				throw new ArgumentNullException(nameof(peImageData));

			byte[] newPEImageData;

			if (fromImageLayout == toImageLayout)
				return peImageData;
			newPEImageData = new byte[GetImageSize(peImageData, toImageLayout)];
			using (PEImage peHeader = new PEImage(peImageData, false)) {
				Buffer.BlockCopy(peImageData, 0, newPEImageData, 0, (int)peHeader.ImageSectionHeaders.Last().EndOffset);
				// 复制PE头
				foreach (ImageSectionHeader sectionHeader in peHeader.ImageSectionHeaders)
					switch (toImageLayout) {
					case ImageLayout.File:
						// ImageLayout.Memory -> ImageLayout.File
						Buffer.BlockCopy(peImageData, (int)sectionHeader.VirtualAddress, newPEImageData, (int)sectionHeader.PointerToRawData, (int)sectionHeader.SizeOfRawData);
						break;
					case ImageLayout.Memory:
						// ImageLayout.File -> ImageLayout.Memory
						Buffer.BlockCopy(peImageData, (int)sectionHeader.PointerToRawData, newPEImageData, (int)sectionHeader.VirtualAddress, (int)sectionHeader.SizeOfRawData);
						break;
					default:
						throw new NotSupportedException();
					}
			}
			return newPEImageData;
		}

		public static uint GetImageSize(byte[] peHeaderData, ImageLayout imageLayout) {
			if (peHeaderData is null)
				throw new ArgumentNullException(nameof(peHeaderData));

			using (PEImage peHeader = new PEImage(peHeaderData, false)) {
				// PEImage构造器中的imageLayout参数无关紧要，因为只需要解析PEHeader
				ImageSectionHeader lastSectionHeader;
				uint alignment;
				uint imageSize;

				lastSectionHeader = peHeader.ImageSectionHeaders.Last();
				switch (imageLayout) {
				case ImageLayout.File:
					alignment = peHeader.ImageNTHeaders.OptionalHeader.FileAlignment;
					imageSize = lastSectionHeader.PointerToRawData + lastSectionHeader.SizeOfRawData;
					break;
				case ImageLayout.Memory:
					alignment = peHeader.ImageNTHeaders.OptionalHeader.SectionAlignment;
					imageSize = (uint)lastSectionHeader.VirtualAddress + lastSectionHeader.VirtualSize;
					break;
				default:
					throw new NotSupportedException();
				}
				if (imageSize % alignment != 0)
					imageSize = imageSize - (imageSize % alignment) + alignment;
				return imageSize;
			}
		}
	}
}
