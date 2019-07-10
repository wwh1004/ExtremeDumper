using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using InternalMetadataInfo = MetadataLocator.MetadataInfo;
using InternalMetadataStreamInfo = MetadataLocator.MetadataStreamInfo;

namespace ExtremeDumper.AntiAntiDump {
	public sealed class MetadataStreamInfo : MarshalByRefObject {
		private readonly uint _rva;
		private readonly uint _length;

		public uint RVA => _rva;

		public uint Length => _length;

		internal unsafe MetadataStreamInfo(InternalMetadataStreamInfo streamInfo, IntPtr moduleHandle) : this((uint)((ulong)streamInfo.Address - (ulong)moduleHandle), streamInfo.Length) {
		}

		internal MetadataStreamInfo(uint rva, uint length) {
			_rva = rva;
			_length = length;
		}
	}

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

	public sealed class MetadataService : MarshalByRefObject {
		public MetadataInfo GetMetadataInfo(IntPtr moduleHandle) {
			Module module;

			module = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetLoadedModules()).First(t => Marshal.GetHINSTANCE(t) == moduleHandle);
			return module is null ? null : new MetadataInfo(new InternalMetadataInfo(module));
		}
	}
}
