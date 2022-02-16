using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using dnlib.DotNet;
using dnlib.IO;
using dnlib.PE;
using ExtremeDumper.AntiAntiDump;

namespace ExtremeDumper.Dumping;

sealed class AntiAntiDumper : DumperBase {
	#region .NET Structures
#pragma warning disable CS0649
	struct IMAGE_DATA_DIRECTORY {
		public uint VirtualAddress;
		public uint Size;
	}

	struct IMAGE_SECTION_HEADER {
		public unsafe fixed byte Name[8];
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

	struct IMAGE_COR20_HEADER {
		public uint cb;
		public ushort MajorRuntimeVersion;
		public ushort MinorRuntimeVersion;
		public IMAGE_DATA_DIRECTORY MetaData;
		public uint Flags;
		public uint EntryPointTokenOrRVA;
		public IMAGE_DATA_DIRECTORY Resources;
		public IMAGE_DATA_DIRECTORY StrongNameSignature;
		public IMAGE_DATA_DIRECTORY CodeManagerTable;
		public IMAGE_DATA_DIRECTORY VTableFixups;
		public IMAGE_DATA_DIRECTORY ExportAddressTableJumps;
		public IMAGE_DATA_DIRECTORY ManagedNativeHeader;
	}

	struct STORAGESIGNATURE {
		public uint lSignature;
		public ushort iMajorVer;
		public ushort iMinorVer;
		public uint iExtraData;
		public uint iVersionString;
		public unsafe fixed byte pVersion[1];
	}

	struct STORAGEHEADER {
		public byte fFlags;
		public byte pad;
		public ushort iStreams;
	}
#pragma warning restore CS0649
	#endregion

	public AntiAntiDumper(uint processId) : base(processId) {
	}

	public override bool DumpModule(nuint moduleHandle, ImageLayout __imageLayout_dont_use, string filePath) {
		var clients = AADExtensions.EnumerateAADClients(process.Id);
		if (!FindModule(clients, moduleHandle, out var client, out var module))
			return false;

		var peInfo = client.GetPEInfo(module);
		var metadataInfo = client.GetMetadataInfo(module);
		var imageLayout = FindMetadataImageLayout(peInfo, metadataInfo.MetadataAddress);
		if (imageLayout is null)
			throw new InvalidOperationException("Can't find the PEImageLayout where the metadata is located");

		moduleHandle = (nuint)imageLayout.ImageBase;
		var data = DumpModule(process.Id, moduleHandle, peInfo.InMemory);
		if (data is null)
			throw new InvalidOperationException("Can't dump module");

		AddressToRVA(metadataInfo, imageLayout);
		if (peInfo.InMemory)
			FileLayoutToMemoryLayout(ref data, metadataInfo, imageLayout);
		FixSectionHeaders(data);
		FixDotNetHeaders(data, metadataInfo, imageLayout);

		File.WriteAllBytes(filePath, data);
		return true;
	}

	static bool FindModule(IEnumerable<AADClient> clients, nuint moduleHandle, [NotNullWhen(true)] out AADClient? client, [NotNullWhen(true)] out ModuleInfo? module) {
		foreach (var client2 in clients) {
			foreach (var module2 in client2.EnumerateModules()) {
				var peInfo = client2.GetPEInfo(module2);
				if (peInfo.FlatLayout.ImageBase == moduleHandle ||
					peInfo.MappedLayout.ImageBase == moduleHandle ||
					peInfo.LoadedLayout.ImageBase == moduleHandle) {
					client = client2;
					module = module2;
					return true;
				}
			}
		}
		client = null;
		module = null;
		return false;
	}

	static PEImageLayout? FindMetadataImageLayout(PEInfo peInfo, ulong metadataAddress) {
		if (Check(peInfo.FlatLayout, metadataAddress))
			return peInfo.FlatLayout;
		if (Check(peInfo.MappedLayout, metadataAddress))
			return peInfo.MappedLayout;
		if (Check(peInfo.LoadedLayout, metadataAddress))
			return peInfo.LoadedLayout;
		return null;

		static bool Check(PEImageLayout imageLayout, ulong metadataAddress) {
			if (imageLayout.IsInvalid)
				return false;
			return imageLayout.ImageBase <= metadataAddress && metadataAddress < imageLayout.ImageBase + imageLayout.ImageSize;
		}
	}

	static byte[]? DumpModule(uint processId, nuint moduleHandle, bool inMemory) {
		var imageLayout = inMemory ? ImageLayout.File : ImageLayout.Memory;
		var imageLayoutOld = imageLayout;
		var data = PEImageDumper.Dump(processId, moduleHandle, ref imageLayout);
		if (data is null)
			return null;
		Debug2.Assert(imageLayoutOld == imageLayout);
		return data;
	}

	static void AddressToRVA(MetadataInfo metadataInfo, PEImageLayout imageLayout) {
		metadataInfo.MetadataAddress -= imageLayout.ImageBase;
		if (!metadataInfo.TableStream.IsInvalid)
			metadataInfo.TableStream.Address -= imageLayout.ImageBase;
		if (!metadataInfo.StringHeap.IsInvalid)
			metadataInfo.StringHeap.Address -= imageLayout.ImageBase;
		if (!metadataInfo.UserStringHeap.IsInvalid)
			metadataInfo.UserStringHeap.Address -= imageLayout.ImageBase;
		if (!metadataInfo.GuidHeap.IsInvalid)
			metadataInfo.GuidHeap.Address -= imageLayout.ImageBase;
		if (!metadataInfo.BlobHeap.IsInvalid)
			metadataInfo.BlobHeap.Address -= imageLayout.ImageBase;
		imageLayout.CorHeaderAddress -= imageLayout.ImageBase;
	}

	static void FileLayoutToMemoryLayout(ref byte[] data, MetadataInfo metadataInfo, PEImageLayout imageLayout) {
		data = PEImageDumper.ConvertImageLayout(data, ImageLayout.File, ImageLayout.Memory);
		using var peHeader = new PEImage(data, ImageLayout.File, false);
		// 用于转换RVA与FOA，必须指定imageLayout参数为ImageLayout.File
		metadataInfo.MetadataAddress = (uint)peHeader.ToRVA((FileOffset)metadataInfo.MetadataAddress);
		if (!metadataInfo.TableStream.IsInvalid)
			metadataInfo.TableStream.Address = (uint)peHeader.ToRVA((FileOffset)metadataInfo.TableStream.Address);
		if (!metadataInfo.StringHeap.IsInvalid)
			metadataInfo.StringHeap.Address = (uint)peHeader.ToRVA((FileOffset)metadataInfo.StringHeap.Address);
		if (!metadataInfo.UserStringHeap.IsInvalid)
			metadataInfo.UserStringHeap.Address = (uint)peHeader.ToRVA((FileOffset)metadataInfo.UserStringHeap.Address);
		if (!metadataInfo.GuidHeap.IsInvalid)
			metadataInfo.GuidHeap.Address = (uint)peHeader.ToRVA((FileOffset)metadataInfo.GuidHeap.Address);
		if (!metadataInfo.BlobHeap.IsInvalid)
			metadataInfo.BlobHeap.Address = (uint)peHeader.ToRVA((FileOffset)metadataInfo.BlobHeap.Address);
		imageLayout.CorHeaderAddress = (uint)peHeader.ToRVA((FileOffset)imageLayout.CorHeaderAddress);
	}

	static unsafe void FixSectionHeaders(byte[] data) {
		using var peHeader = new PEImage(data, ImageLayout.Memory, false);
		fixed (byte* p = data) {
			var pOptionalHeader = p + (uint)peHeader.ImageNTHeaders.OptionalHeader.StartOffset;
			*(uint*)(pOptionalHeader + 0x24) = *(uint*)(pOptionalHeader + 0x20);
			uint alignmentMask = *(uint*)(pOptionalHeader + 0x24) - 1;
			foreach (var sectionHeader in peHeader.ImageSectionHeaders) {
				var pSectionHeader = (IMAGE_SECTION_HEADER*)(p + (uint)sectionHeader.StartOffset);
				pSectionHeader->PointerToRawData = pSectionHeader->VirtualAddress;
				pSectionHeader->SizeOfRawData = (pSectionHeader->VirtualSize + alignmentMask) & ~alignmentMask;
			}
		}
	}

	static unsafe void FixDotNetHeaders(byte[] data, MetadataInfo metadataInfo, PEImageLayout imageLayout) {
		fixed (byte* p = data) {
			var pNETDirectory = (IMAGE_DATA_DIRECTORY*)(p + GetDotNetDirectoryRVA(data));
			pNETDirectory->VirtualAddress = (uint)imageLayout.CorHeaderAddress;
			pNETDirectory->Size = (uint)sizeof(IMAGE_COR20_HEADER);
			// Set Data Directories
			var pCor20Header = (IMAGE_COR20_HEADER*)(p + (uint)imageLayout.CorHeaderAddress);
			pCor20Header->cb = (uint)sizeof(IMAGE_COR20_HEADER);
			pCor20Header->MajorRuntimeVersion = 0x2;
			pCor20Header->MinorRuntimeVersion = 0x5;
			pCor20Header->MetaData.VirtualAddress = (uint)metadataInfo.MetadataAddress;
			pCor20Header->MetaData.Size = metadataInfo.MetadataSize;
			// Set .NET Directory
			var pStorageSignature = (STORAGESIGNATURE*)(p + (uint)metadataInfo.MetadataAddress);
			pStorageSignature->lSignature = 0x424A5342;
			pStorageSignature->iMajorVer = 0x1;
			pStorageSignature->iMinorVer = 0x1;
			pStorageSignature->iExtraData = 0x0;
			pStorageSignature->iVersionString = 0xC;
			var versionString = Encoding.ASCII.GetBytes("v4.0.30319");
			for (int i = 0; i < versionString.Length; i++)
				pStorageSignature->pVersion[i] = versionString[i];
			// versionString仅仅占位用，程序集具体运行时版本用dnlib获取
			// Set StorageSignature
			var pStorageHeader = (STORAGEHEADER*)((byte*)pStorageSignature + 0x10 + pStorageSignature->iVersionString);
			pStorageHeader->fFlags = 0x0;
			pStorageHeader->pad = 0x0;
			pStorageHeader->iStreams = 0x5;
			// Set StorageHeader
			var pStreamHeader = (uint*)((byte*)pStorageHeader + sizeof(STORAGEHEADER));
			var tableStream = metadataInfo.TableStream;
			if (!tableStream.IsInvalid) {
				*pStreamHeader = (uint)tableStream.Address;
				*pStreamHeader -= (uint)metadataInfo.MetadataAddress;
				pStreamHeader++;
				*pStreamHeader = tableStream.Length;
				pStreamHeader++;
				*pStreamHeader = tableStream.IsCompressed ? 0x00007E23u : 0x000002D23;
				pStreamHeader++;
			}
			// Set #~ or #-
			var stringHeap = metadataInfo.StringHeap;
			if (!stringHeap.IsInvalid) {
				*pStreamHeader = (uint)stringHeap.Address;
				*pStreamHeader -= (uint)metadataInfo.MetadataAddress;
				pStreamHeader++;
				*pStreamHeader = stringHeap.Length;
				pStreamHeader++;
				*pStreamHeader = 0x72745323;
				pStreamHeader++;
				*pStreamHeader = 0x73676E69;
				pStreamHeader++;
				*pStreamHeader = 0x00000000;
				pStreamHeader++;
			}
			// Set #Strings
			var userStringHeap = metadataInfo.UserStringHeap;
			if (!userStringHeap.IsInvalid) {
				*pStreamHeader = (uint)userStringHeap.Address;
				*pStreamHeader -= (uint)metadataInfo.MetadataAddress;
				pStreamHeader++;
				*pStreamHeader = userStringHeap.Length;
				pStreamHeader++;
				*pStreamHeader = 0x00535523;
				pStreamHeader++;
			}
			// Set #US
			var guidHeap = metadataInfo.GuidHeap;
			if (!guidHeap.IsInvalid) {
				*pStreamHeader = (uint)guidHeap.Address;
				*pStreamHeader -= (uint)metadataInfo.MetadataAddress;
				pStreamHeader++;
				*pStreamHeader = guidHeap.Length;
				pStreamHeader++;
				*pStreamHeader = 0x49554723;
				pStreamHeader++;
				*pStreamHeader = 0x00000044;
				pStreamHeader++;
			}
			// Set #GUID
			var blobHeap = metadataInfo.BlobHeap;
			if (!blobHeap.IsInvalid) {
				*pStreamHeader = (uint)blobHeap.Address;
				*pStreamHeader -= (uint)metadataInfo.MetadataAddress;
				pStreamHeader++;
				*pStreamHeader = blobHeap.Length;
				pStreamHeader++;
				*pStreamHeader = 0x6F6C4223;
				pStreamHeader++;
				*pStreamHeader = 0x00000062;
				pStreamHeader++;
			}
			// Set #GUID
			switch (GetCorLibVersion(data).Major) {
			case 2:
				versionString = Encoding.ASCII.GetBytes("v2.0.50727");
				break;
			case 4:
				versionString = Encoding.ASCII.GetBytes("v4.0.30319");
				break;
			default:
				throw new NotSupportedException();
			}
			for (int i = 0; i < versionString.Length; i++)
				pStorageSignature->pVersion[i] = versionString[i];
			// Re set Version
		}
	}

	static uint GetDotNetDirectoryRVA(byte[] data) {
		using var peHeader = new PEImage(data, false);
		return (uint)peHeader.ImageNTHeaders.OptionalHeader.DataDirectories[14].StartOffset;
	}

	static Version GetCorLibVersion(byte[] data) {
		using var module = ModuleDefMD.Load(new PEImage(data, ImageLayout.Memory, false));
		return module.CorLibTypes.AssemblyRef.Version;
	}

	public override int DumpProcess(string directoryPath) {
		throw new NotSupportedException();
	}
}
