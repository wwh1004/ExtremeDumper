using System;
using System.Linq;
using dnlib.PE;
using NativeSharp;

namespace ExtremeDumper.Dumper {
	internal static unsafe class PEImageHelper {
		public static byte[] DirectCopy(uint processId, void* moduleHandle, ImageLayout imageLayout, string alternativeToImagePath = null) {
			if (processId == 0)
				throw new ArgumentNullException(nameof(processId));
			if (moduleHandle is null)
				throw new ArgumentNullException(nameof(moduleHandle));

			using (NativeProcess process = NativeProcess.Open(processId)) {
				NativeModule module;
				string imagePath;
				PageInfo firstPageInfo;
				byte[] peImageData;
				uint imageSize;

				module = process.UnsafeGetModule(moduleHandle);
				module.
				imagePath =process.GetModule()
				firstPageInfo = process.EnumeratePageInfos(moduleHandle, moduleHandle).First();
				peImageData = new byte[(uint)firstPageInfo.Size];
				process.ReadBytes(moduleHandle, peImageData);
				imageSize = GetImageSize(peImageData, imageLayout);
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
