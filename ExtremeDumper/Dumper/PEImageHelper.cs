using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using dnlib.IO;
using dnlib.PE;
using NativeSharp;

namespace ExtremeDumper.Dumper {
	internal static unsafe class PEImageHelper {
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct IMAGE_SECTION_HEADER {
			public static readonly uint UnmanagedSize = (uint)sizeof(IMAGE_SECTION_HEADER);

			public ulong Name;
			public uint VirtualSize;
			public uint VirtualAddress;
			public uint SizeOfRawData;
			public uint PointerToRawData;
			public uint PointerToRelocations;
			public uint PointerToLinenumbers;
			public ushort NumberOfRelocations;
			public ushort NumberOfLinenumbers;
			public uint Characteristics;
		}

		/// <summary>
		/// 直接从内存中复制模块，不执行格式转换操作
		/// </summary>
		/// <param name="module">模块</param>
		/// <param name="imageLayout">模块在内存中的格式</param>
		/// <param name="rebuildSectionHeaders">是否重建节表</param>
		/// <returns></returns>
		public static byte[] DirectCopy(NativeModule module, ImageLayout imageLayout, bool rebuildSectionHeaders) {
			if (module is null)
				throw new ArgumentNullException(nameof(module));

			NativeProcess process;
			PageInfo firstPageInfo;
			byte[] peImageData;
			uint imageSize;
			List<IMAGE_SECTION_HEADER> sectionHeaders;

			process = module.Process;
			process.QuickDemand(ProcessAccess.MemoryRead | ProcessAccess.QueryInformation);
			firstPageInfo = process.EnumeratePageInfos(module.Handle, module.Handle).First();
			if (rebuildSectionHeaders) {
				imageSize = 0;
				sectionHeaders = new List<IMAGE_SECTION_HEADER>();
				foreach (PageInfo pageInfo in process.EnumeratePageInfos(module.Handle, (void*)-1)) {
					uint pageSize;

					if (pageInfo.Protection == MemoryProtection.NoAccess)
						break;
					pageSize = (uint)pageInfo.Size;
					if (imageSize != 0)
						// 跳过PE头
						sectionHeaders.Add(new IMAGE_SECTION_HEADER {
							Name = 0, // TODO 自动获取
							VirtualSize = pageSize,
							VirtualAddress = imageSize,
							SizeOfRawData = pageSize,
							PointerToRawData = imageSize
						});
					imageSize += pageSize;
				}
			}
			else {
				byte[] peHeaderData;

				peHeaderData = new byte[(uint)firstPageInfo.Size];
				process.ReadBytes(module.Handle, peHeaderData);
				imageSize = GetImageSize(peHeaderData, imageLayout);
				sectionHeaders = null;
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
			if (rebuildSectionHeaders) {
				int sectionAlignmentOffset;
				int fileAlignmentOffset;
				int sectionHeadersOffset;

				using (PEImage peHeader = new PEImage(peImageData, false)) {
					sectionAlignmentOffset = (int)(peHeader.ImageNTHeaders.OptionalHeader.StartOffset + 32);
					fileAlignmentOffset = sectionAlignmentOffset + 4;
					sectionHeadersOffset = (int)(peHeader.ImageNTHeaders.OptionalHeader.StartOffset + peHeader.ImageNTHeaders.FileHeader.SizeOfOptionalHeader);
				}
				fixed (byte* p = peImageData) {
					*(uint*)(p + sectionAlignmentOffset) = 0x2000;
					*(uint*)(p + fileAlignmentOffset) = 0x2000;
					// TODO
					for (int i = 0; i < sectionHeaders.Count; i++)
						*((IMAGE_SECTION_HEADER*)(p + sectionHeadersOffset) + i) = sectionHeaders[i];
				}
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
