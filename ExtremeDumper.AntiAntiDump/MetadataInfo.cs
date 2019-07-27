using System;
using System.Runtime.InteropServices;
using MetadataLocator;
using InternalDotNetPEInfo = MetadataLocator.DotNetPEInfo;
using InternalMetadataInfo = MetadataLocator.MetadataInfo;
using InternalMetadataStreamInfo = MetadataLocator.MetadataStreamInfo;

namespace ExtremeDumper.AntiAntiDump {
	/// <summary>
	/// Metadata stream info
	/// </summary>
	[Serializable]
	public sealed class MetadataStreamInfo {
		/// <summary />
		public uint Rva;

		/// <summary />
		public uint Length;

		/// <summary />
		public MetadataStreamInfo() {
		}

		internal unsafe MetadataStreamInfo(InternalMetadataStreamInfo streamInfo, IntPtr moduleHandle) {
			Rva = (uint)((byte*)streamInfo.Address - (byte*)moduleHandle);
			Length = streamInfo.Length;
		}
	}

	/// <summary>
	/// .NET PE Info
	/// </summary>
	[Serializable]
	public sealed class DotNetPEInfo {
		/// <summary>
		/// Determine if current instance is valid
		/// </summary>
		public bool IsValid;

		/// <summary>
		/// ImageLayout
		/// </summary>
		public ImageLayout ImageLayout;

		/// <summary>
		/// Rva of COR20_HEADER
		/// NOTICE: It is calculated by (pCor20Header - methodHandle). So you can't direct use it to fill the field ".NET Metadata Directory Rva"
		/// </summary>
		public uint Cor20HeaderRva;

		/// <summary>
		/// Rva of metadata
		/// NOTICE: It is calculated by (pMetadata - methodHandle). So you can't direct use it to fill the field "Metadata Rva"
		/// </summary>
		public uint MetadataRva;

		/// <summary>
		/// Size of metadata
		/// </summary>
		public uint MetadataSize;

		/// <summary />
		public DotNetPEInfo() {
		}

		internal unsafe DotNetPEInfo(InternalDotNetPEInfo peInfo, IntPtr moduleHandle) {
			IsValid = peInfo.IsValid;
			ImageLayout = peInfo.ImageLayout;
			Cor20HeaderRva = (uint)((byte*)peInfo.Cor20HeaderAddress - (byte*)moduleHandle);
			MetadataRva = (uint)((byte*)peInfo.MetadataAddress - (byte*)moduleHandle);
			MetadataSize = peInfo.MetadataSize;
		}
	}

	/// <summary>
	/// Metadata info
	/// </summary>
	[Serializable]
	public sealed class MetadataInfo {
		/// <summary>
		/// #~ or #- info
		/// </summary>
		public MetadataStreamInfo TableStream;

		/// <summary>
		/// #Strings heap info
		/// </summary>
		public MetadataStreamInfo StringHeap;

		/// <summary>
		/// #US heap info
		/// </summary>
		public MetadataStreamInfo UserStringHeap;

		/// <summary>
		/// #GUID heap info
		/// </summary>
		public MetadataStreamInfo GuidHeap;

		/// <summary>
		/// #Blob heap info
		/// </summary>
		public MetadataStreamInfo BlobHeap;

		/// <summary>
		/// .NET PE Info (invalid if PEInfo.IsNativeImage is true)
		/// </summary>
		public DotNetPEInfo PEInfo;

		/// <summary />
		public MetadataInfo() {
		}

		internal MetadataInfo(InternalMetadataInfo metadataInfo) {
			if (metadataInfo is null)
				throw new ArgumentNullException(nameof(metadataInfo));

			IntPtr moduleHandle;

			moduleHandle = Marshal.GetHINSTANCE(metadataInfo.Module);
			if (!(metadataInfo.TableStream is null))
				TableStream = new MetadataStreamInfo(metadataInfo.TableStream, moduleHandle);
			if (!(metadataInfo.StringHeap is null))
				StringHeap = new MetadataStreamInfo(metadataInfo.StringHeap, moduleHandle);
			if (!(metadataInfo.UserStringHeap is null))
				UserStringHeap = new MetadataStreamInfo(metadataInfo.UserStringHeap, moduleHandle);
			if (!(metadataInfo.GuidHeap is null))
				GuidHeap = new MetadataStreamInfo(metadataInfo.GuidHeap, moduleHandle);
			if (!(metadataInfo.BlobHeap is null))
				BlobHeap = new MetadataStreamInfo(metadataInfo.BlobHeap, moduleHandle);
			PEInfo = new DotNetPEInfo(metadataInfo.PEInfo, moduleHandle);
		}
	}
}
