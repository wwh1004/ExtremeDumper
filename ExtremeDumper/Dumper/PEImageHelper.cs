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
		/// <param name="processId">进程ID</param>
		/// <param name="moduleHandle">模块句柄</param>
		/// <param name="imageLayout">模块在内存中的格式</param>
		/// <param name="alternativeToImagePath">如果无法正常获取模块路径，可提供备选模块路径</param>
		/// <param name="fixSectionHeaders">是否修复节头</param>
		/// <returns></returns>
		public static byte[] DirectCopy(uint processId, void* moduleHandle, ImageLayout imageLayout, string alternativeToImagePath, bool fixSectionHeaders) {
			if (processId == 0)
				throw new ArgumentNullException(nameof(processId));
			if (moduleHandle is null)
				throw new ArgumentNullException(nameof(moduleHandle));
			if (string.IsNullOrEmpty(alternativeToImagePath))
				alternativeToImagePath = null;
			else {
				if (!File.Exists(alternativeToImagePath))
					throw new FileNotFoundException(nameof(alternativeToImagePath));
			}

			using (NativeProcess process = NativeProcess.Open(processId)) {
				NativeModule module;
				string imagePath;
				bool hasPhysicalImage;
				PageInfo firstPageInfo;
				byte[] peImageData;
				uint imageSize;

				module = process.UnsafeGetModule(moduleHandle);
				imagePath = module.ImagePath;
				if (string.IsNullOrEmpty(imagePath))
					imagePath = alternativeToImagePath;
				hasPhysicalImage = !string.IsNullOrEmpty(imagePath);
				// 获取模块路径
				firstPageInfo = process.EnumeratePageInfos(moduleHandle, moduleHandle).First();
				if (hasPhysicalImage)
					imageSize = GetImageSize(File.ReadAllBytes(imagePath), imageLayout);
				else {
					byte[] peHeaderData;

					peHeaderData = new byte[(uint)firstPageInfo.Size];
					process.ReadBytes(moduleHandle, peHeaderData);
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
					foreach (PageInfo pageInfo in process.EnumeratePageInfos(moduleHandle, (byte*)moduleHandle + imageSize)) {
						uint offset;

						offset = (uint)((ulong)pageInfo.Address - (ulong)moduleHandle);
						process.TryReadBytes(pageInfo.Address, peImageData, offset, (uint)pageInfo.Size);
					}
					break;
				default:
					throw new NotSupportedException();
				}
				if (fixSectionHeaders && hasPhysicalImage)
					using (PEImage peHeader = new PEImage(imagePath, false)) {
						int startOffset;
						int endOffset;
						byte[] sectionHeadersData;

						startOffset = (int)peHeader.ImageSectionHeaders.First().StartOffset;
						endOffset = (int)peHeader.ImageSectionHeaders.Last().EndOffset;
						sectionHeadersData = peHeader.CreateReader((FileOffset)startOffset).ReadBytes(endOffset - startOffset);
						Buffer.BlockCopy(sectionHeadersData, 0, peImageData, startOffset, endOffset - startOffset);
					}
				return peImageData;
			}
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
