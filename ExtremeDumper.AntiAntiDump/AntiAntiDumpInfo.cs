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
	public sealed class MetadataStreamInfo : MarshalByRefObject {
		private readonly uint _rva;
		private readonly uint _length;

		public uint Rva => _rva;

		public uint Length => _length;

		internal unsafe MetadataStreamInfo(InternalMetadataStreamInfo streamInfo, IntPtr moduleHandle) : this((uint)((byte*)streamInfo.Address - (byte*)moduleHandle), streamInfo.Length) {
		}

		internal MetadataStreamInfo(uint rva, uint length) {
			_rva = rva;
			_length = length;
		}
	}

	/// <summary>
	/// Metadata info
	/// </summary>
	public sealed class MetadataInfo : MarshalByRefObject {
		private readonly MetadataStreamInfo _tableStream;
		private readonly MetadataStreamInfo _stringHeap;
		private readonly MetadataStreamInfo _userStringHeap;
		private readonly MetadataStreamInfo _guidHeap;
		private readonly MetadataStreamInfo _blobHeap;

		public MetadataStreamInfo TableStream => _tableStream;

		public MetadataStreamInfo StringHeap => _stringHeap;

		public MetadataStreamInfo UserStringHeap => _userStringHeap;

		public MetadataStreamInfo GuidHeap => _guidHeap;

		public MetadataStreamInfo BlobHeap => _blobHeap;

		internal MetadataInfo(InternalMetadataInfo metadataInfo) {
			if (metadataInfo is null)
				throw new ArgumentNullException(nameof(metadataInfo));

			IntPtr moduleHandle;

			moduleHandle = Marshal.GetHINSTANCE(metadataInfo.Module);
			if (!(metadataInfo.TableStream is null))
				_tableStream = new MetadataStreamInfo(metadataInfo.TableStream, moduleHandle);
			if (!(metadataInfo.StringHeap is null))
				_stringHeap = new MetadataStreamInfo(metadataInfo.StringHeap, moduleHandle);
			if (!(metadataInfo.UserStringHeap is null))
				_userStringHeap = new MetadataStreamInfo(metadataInfo.UserStringHeap, moduleHandle);
			if (!(metadataInfo.GuidHeap is null))
				_guidHeap = new MetadataStreamInfo(metadataInfo.GuidHeap, moduleHandle);
			if (!(metadataInfo.BlobHeap is null))
				_blobHeap = new MetadataStreamInfo(metadataInfo.BlobHeap, moduleHandle);
		}
	}

	/// <summary>
	/// AntiAntiDump info
	/// </summary>
	public sealed class AntiAntiDumpInfo : MarshalByRefObject {
		private readonly bool _canAntiAntiDump;
		private readonly ImageLayout _imageLayout;
		private readonly uint _cor20HeaderRva;
		private readonly uint _metadataRva;
		private readonly uint _metadataSize;
		private readonly MetadataInfo _metadataInfo;

		/// <summary>
		/// Can AntiAntiDump
		/// </summary>
		public bool CanAntiAntiDump => _canAntiAntiDump;

		/// <summary>
		/// ImageLayout
		/// </summary>
		public ImageLayout ImageLayout => _imageLayout;

		/// <summary>
		/// Rva of COR20_HEADER
		/// NOTICE: It is calculated by (pCor20Header - methodHandle). So you can't direct use it to fill the field ".NET Metadata Directory Rva"
		/// </summary>
		public uint Cor20HeaderRva => _cor20HeaderRva;

		/// <summary>
		/// Rva of metadata
		/// NOTICE: It is calculated by (pMetadata - methodHandle). So you can't direct use it to fill the field "Metadata Rva"
		/// </summary>
		public uint MetadataRva => _metadataRva;

		/// <summary>
		/// Size of metadata
		/// </summary>
		public uint MetadataSize => _metadataSize;

		/// <summary>
		/// Metadata info
		/// </summary>
		public MetadataInfo MetadataInfo => _metadataInfo;

		public AntiAntiDumpInfo(bool canAntiAntiDump, ImageLayout imageLayout, uint cor20HeaderRva, uint metadataRva, uint metadataSize, MetadataInfo metadataInfo) {
			_canAntiAntiDump = canAntiAntiDump;
			_imageLayout = imageLayout;
			_cor20HeaderRva = cor20HeaderRva;
			_metadataRva = metadataRva;
			_metadataSize = metadataSize;
			_metadataInfo = metadataInfo;
		}
	}
}
