using System;
using System.Linq;
using dnlib.PE;
using NativeSharp;

namespace ExtremeDumper.Dumping {
	public static unsafe class PEImageDumper {
		/// <summary>
		/// 直接从内存中复制模块，不执行格式转换操作
		/// </summary>
		/// <param name="processId"></param>
		/// <param name="address"></param>
		/// <param name="imageLayout"></param>
		/// <returns></returns>
		public static byte[] Dump(uint processId, void* address, ref ImageLayout imageLayout) {
			if (processId == 0)
				throw new ArgumentNullException(nameof(processId));
			if (address == null)
				throw new ArgumentNullException(nameof(address));

			using (var process = NativeProcess.Open(processId))
				return Dump(process, address, ref imageLayout);
		}

		/// <summary>
		/// 直接从内存中复制模块，不执行格式转换操作
		/// </summary>
		/// <param name="process"></param>
		/// <param name="address"></param>
		/// <param name="imageLayout"></param>
		/// <returns></returns>
		public static byte[] Dump(NativeProcess process, void* address, ref ImageLayout imageLayout) {
			var firstPageInfo = process.EnumeratePageInfos(address, address).First();
			bool atPageHeader = address == firstPageInfo.Address;
			if (!atPageHeader)
				imageLayout = ImageLayout.File;
			byte[] peHeader = new byte[atPageHeader ? (int)firstPageInfo.Size : (int)((byte*)firstPageInfo.Address + (uint)firstPageInfo.Size - (byte*)address)];
			process.ReadBytes(address, peHeader);
			uint imageSize = GetImageSize(peHeader, imageLayout);
			// 获取模块在内存中的大小

			byte[] peImage = new byte[imageSize];
			switch (imageLayout) {
			case ImageLayout.File:
				if (!process.TryReadBytes(address, peImage, 0, imageSize))
					throw new InvalidOperationException();
				break;
			case ImageLayout.Memory:
				foreach (var pageInfo in process.EnumeratePageInfos(address, (byte*)address + imageSize)) {
					uint offset = (uint)((ulong)pageInfo.Address - (ulong)address);
					process.TryReadBytes(pageInfo.Address, peImage, offset, (uint)pageInfo.Size);
				}
				break;
			default:
				throw new NotSupportedException();
			}
			// 转储

			return peImage;
		}

		/// <summary>
		/// 转换模块布局
		/// </summary>
		/// <param name="peImage"></param>
		/// <param name="fromImageLayout"></param>
		/// <param name="toImageLayout"></param>
		/// <returns></returns>
		public static byte[] ConvertImageLayout(byte[] peImage, ImageLayout fromImageLayout, ImageLayout toImageLayout) {
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
			if (peImage is null)
				throw new ArgumentNullException(nameof(peImage));

			if (fromImageLayout == toImageLayout)
				return peImage;
			byte[] newPEImageData = new byte[GetImageSize(peImage, toImageLayout)];
			using (var peHeader = new PEImage(peImage, false)) {
				Buffer.BlockCopy(peImage, 0, newPEImageData, 0, (int)peHeader.ImageSectionHeaders.Last().EndOffset);
				// 复制PE头
				foreach (var sectionHeader in peHeader.ImageSectionHeaders) {
					switch (toImageLayout) {
					case ImageLayout.File:
						// ImageLayout.Memory -> ImageLayout.File
						Buffer.BlockCopy(peImage, (int)sectionHeader.VirtualAddress, newPEImageData, (int)sectionHeader.PointerToRawData, (int)sectionHeader.SizeOfRawData);
						break;
					case ImageLayout.Memory:
						// ImageLayout.File -> ImageLayout.Memory
						Buffer.BlockCopy(peImage, (int)sectionHeader.PointerToRawData, newPEImageData, (int)sectionHeader.VirtualAddress, (int)sectionHeader.SizeOfRawData);
						break;
					default:
						throw new NotSupportedException();
					}
				}
			}
			return newPEImageData;
		}

		/// <summary>
		/// 获取模块大小
		/// </summary>
		/// <param name="peHeader"></param>
		/// <param name="imageLayout"></param>
		/// <returns></returns>
		public static uint GetImageSize(byte[] peHeader, ImageLayout imageLayout) {
			if (peHeader is null)
				throw new ArgumentNullException(nameof(peHeader));

			using (var peImage = new PEImage(peHeader, false))
				return GetImageSize(peImage, imageLayout);
			// PEImage构造器中的imageLayout参数无关紧要，因为只需要解析PEHeader
		}

		public static uint GetImageSize(PEImage peHeader, ImageLayout imageLayout) {
			var lastSectionHeader = peHeader.ImageSectionHeaders.Last();
			uint alignment;
			uint imageSize;
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
