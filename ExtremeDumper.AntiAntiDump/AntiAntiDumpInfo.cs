using System;
using System.Runtime.InteropServices;
using InternalMetadataInfo = MetadataLocator.MetadataInfo;
using InternalMetadataStreamInfo = MetadataLocator.MetadataStreamInfo;

namespace ExtremeDumper.AntiAntiDump {
	/// <summary>
	/// Image layout
	/// </summary>
	public enum ImageLayout {
		/// <summary>
		/// Use this if the PE file has a normal structure (eg. it's been read from a file on disk)
		/// </summary>
		File,

		/// <summary>
		/// Use this if the PE file has been loaded into memory by the OS PE file loader
		/// </summary>
		Memory
	}

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
	/// Metadata info
	/// </summary>
	[Serializable]
	public sealed class MetadataInfo {
		/// <summary />
		public MetadataStreamInfo TableStream;

		/// <summary />
		public MetadataStreamInfo StringHeap;

		/// <summary />
		public MetadataStreamInfo UserStringHeap;

		/// <summary />
		public MetadataStreamInfo GuidHeap;

		/// <summary />
		public MetadataStreamInfo BlobHeap;

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
		}
	}

	/// <summary>
	/// AntiAntiDump info
	/// </summary>
	[Serializable]
	public sealed class AntiAntiDumpInfo {
		/// <summary>
		/// Can AntiAntiDump
		/// </summary>
		public bool CanAntiAntiDump;

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

		/// <summary>
		/// Metadata info
		/// </summary>
		public MetadataInfo MetadataInfo;
	}
}
