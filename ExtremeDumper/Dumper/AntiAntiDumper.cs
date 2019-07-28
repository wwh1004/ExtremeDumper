using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using dnlib.DotNet;
using dnlib.IO;
using dnlib.PE;
using ExtremeDumper.AntiAntiDump;
using ExtremeDumper.AntiAntiDump.Serialization;
using Microsoft.Diagnostics.Runtime;
using NativeSharp;
using ImageLayout = dnlib.PE.ImageLayout;

namespace ExtremeDumper.Dumper {
	public sealed unsafe class AntiAntiDumper : IDumper {
		#region .net structs
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct IMAGE_DATA_DIRECTORY {
			public static readonly uint UnmanagedSize = (uint)sizeof(IMAGE_DATA_DIRECTORY);

			public uint VirtualAddress;
			public uint Size;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct IMAGE_COR20_HEADER {
			public static readonly uint UnmanagedSize = (uint)sizeof(IMAGE_COR20_HEADER);

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

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct STORAGESIGNATURE {
			/// <summary>
			/// 大小不包括pVersion的长度
			/// </summary>
			public static readonly uint UnmanagedSize = (uint)sizeof(STORAGESIGNATURE) - 1;

			public uint lSignature;
			public ushort iMajorVer;
			public ushort iMinorVer;
			public uint iExtraData;
			public uint iVersionString;
			/// <summary>
			/// 由于C#语法问题不能写pVersion[0]，实际长度由 <see cref="iVersionString"/> 决定
			/// </summary>
			public fixed byte pVersion[1];
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct STORAGEHEADER {
			public static readonly uint UnmanagedSize = (uint)sizeof(STORAGEHEADER);

			public byte fFlags;
			public byte pad;
			public ushort iStreams;
		}
		#endregion

		private uint _processId;

		private AntiAntiDumper() {
		}

		public static IDumper Create(uint processId) {
			return new AntiAntiDumper() {
				_processId = processId
			};
		}

		public bool DumpModule(IntPtr moduleHandle, ImageLayout imageLayout, string filePath) {
			ClrModule dacModule;
			InjectionClrVersion clrVersion;
			InjectionOptions injectionOptions;
			MetadataInfoService metadataInfoService;
			MetadataInfo metadataInfo;
			byte[] peImageData;

			dacModule = TryGetDacModule(moduleHandle);
			if (dacModule is null)
				return false;
			switch (dacModule.Runtime.ClrInfo.Version.Major) {
			case 2:
				clrVersion = InjectionClrVersion.V2;
				break;
			case 4:
				clrVersion = InjectionClrVersion.V4;
				break;
			default:
				return false;
			}
			// 判断要dump的模块的CLR版本
			injectionOptions = new InjectionOptions {
				PortName = Guid.NewGuid().ToString(),
				ObjectName = Guid.NewGuid().ToString()
			};
			using (NativeProcess process = NativeProcess.Open(_processId))
				if (!process.InjectManaged(typeof(MetadataInfoService).Assembly.Location, typeof(Injection).FullName, "Main", XmlSerializer.Serialize(injectionOptions), clrVersion, out int result) || result != 0)
					return false;
			metadataInfoService = (MetadataInfoService)Activator.GetObject(typeof(MetadataInfoService), $"Ipc://{injectionOptions.PortName}/{injectionOptions.ObjectName}");
			// 注入DLL，通过.NET Remoting获取MetadataInfoService实例
			metadataInfo = XmlSerializer.Deserialize<MetadataInfo>(metadataInfoService.GetMetadataInfo(moduleHandle));
			if (!metadataInfo.PEInfo.IsValid)
				return false;
			imageLayout = (ImageLayout)metadataInfo.PEInfo.ImageLayout;
			peImageData = PEImageHelper.DirectCopy(_processId, (void*)moduleHandle, imageLayout, dacModule.FileName, true);
			if (imageLayout == ImageLayout.File)
				// 统一为内存格式，方便修复
				FileLayoutToMemoryLayout(ref peImageData, metadataInfo);
			FixDotNetHeaders(peImageData, metadataInfo);
			// 修复.NET头
			peImageData = PEImageHelper.ConvertImageLayout(peImageData, ImageLayout.Memory, ImageLayout.File);
			// 转换回文件格式用于保存
			File.WriteAllBytes(filePath, peImageData);
			return true;
		}

		private static void FileLayoutToMemoryLayout(ref byte[] peImageData, MetadataInfo metadataInfo) {
			peImageData = PEImageHelper.ConvertImageLayout(peImageData, ImageLayout.File, ImageLayout.Memory);
			using (PEImage peHeader = new PEImage(peImageData, ImageLayout.File, false)) {
				// 用于转换RVA与FOA，必须指定imageLayout参数为ImageLayout.File
				DotNetPEInfo peInfo;

				if (!(metadataInfo.TableStream is null))
					metadataInfo.TableStream.Rva = (uint)peHeader.ToRVA((FileOffset)metadataInfo.TableStream.Rva);
				if (!(metadataInfo.StringHeap is null))
					metadataInfo.StringHeap.Rva = (uint)peHeader.ToRVA((FileOffset)metadataInfo.StringHeap.Rva);
				if (!(metadataInfo.UserStringHeap is null))
					metadataInfo.UserStringHeap.Rva = (uint)peHeader.ToRVA((FileOffset)metadataInfo.UserStringHeap.Rva);
				if (!(metadataInfo.GuidHeap is null))
					metadataInfo.GuidHeap.Rva = (uint)peHeader.ToRVA((FileOffset)metadataInfo.GuidHeap.Rva);
				if (!(metadataInfo.BlobHeap is null))
					metadataInfo.BlobHeap.Rva = (uint)peHeader.ToRVA((FileOffset)metadataInfo.BlobHeap.Rva);
				peInfo = metadataInfo.PEInfo;
				peInfo.ImageLayout = MetadataLocator.ImageLayout.Memory;
				peInfo.Cor20HeaderRva = (uint)peHeader.ToRVA((FileOffset)peInfo.Cor20HeaderRva);
				peInfo.MetadataRva = (uint)peHeader.ToRVA((FileOffset)peInfo.MetadataRva);
			}
		}

		private static void FixDotNetHeaders(byte[] peImageData, MetadataInfo metadataInfo) {
			DotNetPEInfo peInfo;

			peInfo = metadataInfo.PEInfo;
			fixed (byte* p = peImageData) {
				IMAGE_DATA_DIRECTORY* pNETDirectory;
				IMAGE_COR20_HEADER* pCor20Header;
				STORAGESIGNATURE* pStorageSignature;
				byte[] versionString;
				STORAGEHEADER* pStorageHeader;
				uint* pStreamHeader;

				using (PEImage peHeader = new PEImage(peImageData, false))
					pNETDirectory = (IMAGE_DATA_DIRECTORY*)(p + (uint)peHeader.ImageNTHeaders.OptionalHeader.DataDirectories[14].StartOffset);
				pNETDirectory->VirtualAddress = peInfo.Cor20HeaderRva;
				pNETDirectory->Size = IMAGE_COR20_HEADER.UnmanagedSize;
				// Set Data Directories
				pCor20Header = (IMAGE_COR20_HEADER*)(p + peInfo.Cor20HeaderRva);
				pCor20Header->cb = IMAGE_COR20_HEADER.UnmanagedSize;
				pCor20Header->MajorRuntimeVersion = 0x2;
				pCor20Header->MinorRuntimeVersion = 0x5;
				pCor20Header->MetaData.VirtualAddress = peInfo.MetadataRva;
				pCor20Header->MetaData.Size = peInfo.MetadataSize;
				// Set .NET Directory
				pStorageSignature = (STORAGESIGNATURE*)(p + peInfo.MetadataRva);
				pStorageSignature->lSignature = 0x424A5342;
				pStorageSignature->iMajorVer = 0x1;
				pStorageSignature->iMinorVer = 0x1;
				pStorageSignature->iExtraData = 0x0;
				pStorageSignature->iVersionString = 0xC;
				versionString = Encoding.ASCII.GetBytes("v4.0.30319");
				for (int i = 0; i < versionString.Length; i++)
					pStorageSignature->pVersion[i] = versionString[i];
				// versionString仅仅占位用，程序集具体运行时版本用dnlib获取
				// Set StorageSignature
				pStorageHeader = (STORAGEHEADER*)((byte*)pStorageSignature + STORAGESIGNATURE.UnmanagedSize + pStorageSignature->iVersionString);
				pStorageHeader->fFlags = 0x0;
				pStorageHeader->pad = 0x0;
				pStorageHeader->iStreams = 0x5;
				// Set StorageHeader
				pStreamHeader = (uint*)((byte*)pStorageHeader + STORAGEHEADER.UnmanagedSize);
				if (!(metadataInfo.TableStream is null)) {
					*pStreamHeader = metadataInfo.TableStream.Rva;
					*pStreamHeader -= peInfo.MetadataRva;
					pStreamHeader++;
					*pStreamHeader = metadataInfo.TableStream.Length;
					pStreamHeader++;
					*pStreamHeader = 0x00007E23;
					// #~ 暂时不支持#-表流的程序集
					pStreamHeader++;
				}
				if (!(metadataInfo.StringHeap is null)) {
					*pStreamHeader = metadataInfo.StringHeap.Rva;
					*pStreamHeader -= peInfo.MetadataRva;
					pStreamHeader++;
					*pStreamHeader = metadataInfo.StringHeap.Length;
					pStreamHeader++;
					*pStreamHeader = 0x72745323;
					pStreamHeader++;
					*pStreamHeader = 0x73676E69;
					pStreamHeader++;
					*pStreamHeader = 0x00000000;
					pStreamHeader++;
					// #Strings
				}
				if (!(metadataInfo.UserStringHeap is null)) {
					*pStreamHeader = metadataInfo.UserStringHeap.Rva;
					*pStreamHeader -= peInfo.MetadataRva;
					pStreamHeader++;
					*pStreamHeader = metadataInfo.UserStringHeap.Length;
					pStreamHeader++;
					*pStreamHeader = 0x00535523;
					pStreamHeader++;
					// #US
				}
				if (!(metadataInfo.GuidHeap is null)) {
					*pStreamHeader = metadataInfo.GuidHeap.Rva;
					*pStreamHeader -= peInfo.MetadataRva;
					pStreamHeader++;
					*pStreamHeader = metadataInfo.GuidHeap.Length;
					pStreamHeader++;
					*pStreamHeader = 0x49554723;
					pStreamHeader++;
					*pStreamHeader = 0x00000044;
					pStreamHeader++;
					// #GUID
				}
				if (!(metadataInfo.BlobHeap is null)) {
					*pStreamHeader = metadataInfo.BlobHeap.Rva;
					*pStreamHeader -= peInfo.MetadataRva;
					pStreamHeader++;
					*pStreamHeader = metadataInfo.BlobHeap.Length;
					pStreamHeader++;
					*pStreamHeader = 0x6F6C4223;
					pStreamHeader++;
					*pStreamHeader = 0x00000062;
					pStreamHeader++;
					// #GUID
				}
			}
			using (ModuleDefMD moduleDef = ModuleDefMD.Load(new PEImage(peImageData, ImageLayout.Memory, false)))
				fixed (byte* p = peImageData) {
					STORAGESIGNATURE* pStorageSignature;
					byte[] versionString;

					pStorageSignature = (STORAGESIGNATURE*)(p + peInfo.MetadataRva);
					switch (moduleDef.CorLibTypes.AssemblyRef.Version.Major) {
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
				}
		}

		private ClrModule TryGetDacModule(IntPtr moduleHandle) {
			DataTarget dataTarget;

			try {
				using (dataTarget = DataTarget.AttachToProcess((int)_processId, 3000, AttachFlag.Passive))
					return dataTarget.ClrVersions.SelectMany(t => t.CreateRuntime().Modules).First(t => (IntPtr)t.ImageBase == moduleHandle);
			}
			catch {
			}
			return null;
		}

		public int DumpProcess(string directoryPath) {
			throw new NotSupportedException();
		}

		public void Dispose() {
		}
	}
}
