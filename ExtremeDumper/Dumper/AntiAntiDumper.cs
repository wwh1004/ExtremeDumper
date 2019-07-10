using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using dnlib.PE;
using ExtremeDumper.AntiAntiDump;
using Microsoft.Diagnostics.Runtime;
using NativeSharp;

namespace ExtremeDumper.Dumper {
	public sealed unsafe class AntiAntiDumper : IDumper {
		private uint _processId;

		private AntiAntiDumper() {
		}

		public static IDumper Create(uint processId) {
			return new AntiAntiDumper() {
				_processId = processId
			};
		}

		public bool DumpModule(IntPtr moduleHandle, string filePath) {
			return false;
			Injection.Options options;
			MetadataService metadataService;
			MetadataInfo metadataInfo;
			byte[] peImage;

			options = new Injection.Options {
				PortName = Guid.NewGuid().ToString(),
				ObjectName = Guid.NewGuid().ToString()
			};
			using (NativeProcess process = NativeProcess.Open(_processId))
				if (!process.InjectManaged(typeof(MetadataService).Assembly.Location, typeof(Injection).FullName, "Main", options.Serialize(), out int result) || result != 0)
					return false;
			metadataService = (MetadataService)Activator.GetObject(typeof(MetadataService), $"Ipc://{options.PortName}/{options.ObjectName}");
			metadataInfo = metadataService.GetMetadataInfo(moduleHandle);
			PrintStreamInfo("#~ or #-", metadataInfo.TableStream);
			PrintStreamInfo("#Strings", metadataInfo.StringHeap);
			PrintStreamInfo("#US", metadataInfo.UserStringHeap);
			PrintStreamInfo("#GUID", metadataInfo.GuidHeap);
			PrintStreamInfo("#Blob", metadataInfo.BlobHeap);
			peImage = DumpMemoryModule(moduleHandle);
			return true;
		}

		private static void PrintStreamInfo(string name, MetadataStreamInfo streamInfo) {
			Debug.WriteLine($"Name: {name}");
			if (streamInfo == null) {
				Debug.WriteLine("Not exists.");
			}
			else {
				Debug.WriteLine($"Address: 0x{streamInfo.RVA.ToString("X8")}");
				Debug.WriteLine($"Length: 0x{streamInfo.Length.ToString("X8")}");
			}
			Debug.WriteLine(string.Empty);
		}

		public int DumpProcess(string directoryPath) {
			throw new NotSupportedException();
		}

		private byte[] DumpMemoryModule(IntPtr moduleHandle) {
			using (NativeProcess process = NativeProcess.Open(_processId)) {
				PageInfo firstPageInfo;
				byte[] buffer;
				uint imageSize;

				firstPageInfo = process.EnumeratePageInfos(moduleHandle, moduleHandle).First();
				buffer = new byte[(uint)firstPageInfo.Size];
				process.ReadBytes(moduleHandle, buffer);
				imageSize = GetImageSize(buffer);
				buffer = new byte[imageSize];
				foreach (PageInfo pageInfo in process.EnumeratePageInfos(moduleHandle, (IntPtr)((ulong)moduleHandle + imageSize))) {
					uint offset;

					offset = (uint)((ulong)pageInfo.Address - (ulong)moduleHandle);
					process.TryReadBytes(pageInfo.Address, buffer, offset, (uint)pageInfo.Size);
				}
				return buffer;
			}
		}

		private static uint GetImageSize(byte[] header) {
			using (PEImage peImage = new PEImage(header, ImageLayout.Memory, false)) {
				ImageSectionHeader lastSectionHeader;
				uint sectionAlignment;
				uint imageSize;

				lastSectionHeader = peImage.ImageSectionHeaders.Last();
				sectionAlignment = peImage.ImageNTHeaders.OptionalHeader.SectionAlignment;
				imageSize = (uint)lastSectionHeader.VirtualAddress + lastSectionHeader.VirtualSize;
				if (imageSize % sectionAlignment != 0)
					imageSize = imageSize - (imageSize % sectionAlignment) + sectionAlignment;
				return imageSize;
			}
		}

		//private  byte[] FixHeader( byte[] data,IntPtr moduleHandle) {
		//	const uint NEW_SECTION_SIZE = 0x1000;

		//	using (PEImage peImage = new PEImage(data, ImageLayout.Memory, false)) {
		//		byte[] buffer;

		//		buffer = new byte[(uint)data.Length + NEW_SECTION_SIZE];
		//		Buffer.BlockCopy(data, 0, buffer, 0, data.Length);
		//		fixed (byte* pBase = buffer) {
		//			uint offset;
		//			ImageSectionHeader lastSectionHeader;
		//			uint value4;
		//			uint newSectionRVA;
		//			uint metadataRVA;
		//			uint metadataSize;

		//			offset = (uint)peImage.ImageNTHeaders.OptionalHeader.DataDirectories[14].StartOffset;
		//			lastSectionHeader = peImage.ImageSectionHeaders.Last();
		//			newSectionRVA = default;
		//			for (uint i = 0; i < 0x28; i++) {
		//				switch (i) {
		//				case 0x8:
		//					// Virtual Size
		//					value4 = NEW_SECTION_SIZE;
		//					break;
		//				case 0xC:
		//					// Virtual Address
		//					newSectionRVA = (uint)lastSectionHeader.VirtualAddress + lastSectionHeader.VirtualSize;
		//					value4 = newSectionRVA;
		//					break;
		//				default:
		//					value4 = 0;
		//					break;
		//				}
		//				*(uint*)(pBase + offset + i) = value4;
		//			}
		//			// Add new section
		//			value4 = newSectionRVA;
		//			*(uint*)(pBase + offset) = value4;
		//			// Set .NET Metadata Directory RVA
		//			*(uint*)(pBase + 4) = 0x48;
		//			// Set .NET Metadata Directory Size
		//			if (!TryGetMetadataInfoByDac(moduleHandle, out metadataRVA, out metadataSize)) {
		//				metadataRVA =
		//			}
		//			// Get Metadata Info
		//			offset = value4;
		//			for (uint i = 0; i < 0x48; i += 4) {
		//				switch (i) {
		//				case 0x00:
		//					// cb
		//					value4 = 0x48;
		//					break;
		//				case 0x04:
		//					// MajorRuntimeVersion
		//					// MinorRuntimeVersion
		//					value4 = 0x00050002;
		//					break;
		//				case 0x08:
		//					// Metadata RVA
		//					value4 = metadataRVA;
		//					break;
		//				case 0x0C:
		//					// Metadata Size
		//					value4 = metadataSize;
		//					break;
		//				case 0x10:
		//					// Flags
		//					value4 = 0;
		//					break;
		//				case 0x14:
		//					// EntryPointTokenOrRVA
		//					value4 = 0x6000001;
		//					// 随便写一个
		//					break;
		//				default:
		//					value4 = 0;
		//					break;
		//				}
		//				*(uint*)(pBase + offset + i) = value4;
		//			}
		//			// Set .Net Metadata Directory
		//		}
		//	}
		//}

		private bool TryGetMetadataInfoByDac(IntPtr moduleHandle,out uint metadataRVA,out uint metadataSize) {
			ClrModule dacModule;

			metadataRVA = default;
			metadataSize = default;
			dacModule = GetDacModule(moduleHandle);
			if (dacModule == null)
				return false;
			try {
				metadataRVA = (uint)(dacModule.MetadataAddress - (ulong)moduleHandle);
				metadataSize = (uint)dacModule.MetadataLength;
			}
			catch {
				return false;
			}
			return true;
		}

		private ClrModule GetDacModule(IntPtr moduleHandle) {
			DataTarget dataTarget;

			try {
				using (dataTarget = DataTarget.AttachToProcess((int)_processId, 3000, AttachFlag.Passive))
					return dataTarget.ClrVersions.SelectMany(t => t.CreateRuntime().Modules).First(t => (IntPtr)t.ImageBase == moduleHandle);
			}
			catch {
			}
			return null;
		}

		public void Dispose() {
		}
	}
}
