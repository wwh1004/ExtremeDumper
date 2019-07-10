using System;
using System.Reflection;

namespace MetadataLocator {
	/// <summary>
	/// Metadata info
	/// </summary>
	internal sealed unsafe class MetadataInfo {
		private readonly Module _module;
		private readonly IMetaDataTables _metaDataTables;
		private MetadataStreamInfo _tableStream;
		private bool _isTableStreamInitialized;
		private MetadataStreamInfo _stringHeap;
		private bool _isStringHeapInitialized;
		private MetadataStreamInfo _userStringHeap;
		private bool _isUserStringHeapInitialized;
		private MetadataStreamInfo _guidHeap;
		private bool _isGuidHeapInitialized;
		private MetadataStreamInfo _blobHeap;
		private bool _isBlobHeapInitialized;
		// 使用_isXXXInitialized的原因是也许元数据流会为空，所以不通过判断字段是否为null来初始化字段

		/// <summary>
		/// Module
		/// </summary>
		public Module Module => _module;

		/// <summary>
		/// The instance of <see cref="IMetaDataTables"/>
		/// </summary>
		public IMetaDataTables MetaDataTables => _metaDataTables;

		/// <summary>
		/// #~ or #- info
		/// </summary>
		public MetadataStreamInfo TableStream {
			get {
				if (!_isTableStreamInitialized) {
					_tableStream = GetTableStream();
					_isTableStreamInitialized = true;
				}
				return _tableStream;
			}
		}

		/// <summary>
		/// #Strings heap info
		/// </summary>
		public MetadataStreamInfo StringHeap {
			get {
				if (!_isStringHeapInitialized) {
					_stringHeap = GetStringHeap();
					_isStringHeapInitialized = true;
				}
				return _stringHeap;
			}
		}

		/// <summary>
		/// #US heap info
		/// </summary>
		public MetadataStreamInfo UserStringHeap {
			get {
				if (!_isUserStringHeapInitialized) {
					_userStringHeap = GetUserStringHeap();
					_isUserStringHeapInitialized = true;
				}
				return _userStringHeap;
			}
		}

		/// <summary>
		/// #GUID heap info
		/// </summary>
		public MetadataStreamInfo GuidHeap {
			get {
				if (!_isGuidHeapInitialized) {
					_guidHeap = GetGuidHeap();
					_isGuidHeapInitialized = true;
				}
				return _guidHeap;
			}
		}

		/// <summary>
		/// #Blob heap info
		/// </summary>
		public MetadataStreamInfo BlobHeap {
			get {
				if (!_isBlobHeapInitialized) {
					_blobHeap = GetBlobHeap();
					_isBlobHeapInitialized = true;
				}
				return _blobHeap;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="module">A module to get metadata</param>
		public MetadataInfo(Module module) {
			if (module is null)
				throw new ArgumentNullException(nameof(module));

			_module = module;
			_metaDataTables = MetadataHelper.GetIMetaDataTables(MetadataHelper.GetMetadataImport(module));
			if (_metaDataTables is null)
				throw new InvalidOperationException();
		}

		private MetadataStreamInfo GetTableStream() {
			uint tableCount;
			uint tablesSize;
			uint validTableCount;
			uint headerSize;
			void* address;
			uint size;

			ThrowOnError(_metaDataTables.GetNumTables(out tableCount));
			tablesSize = 0;
			validTableCount = 0;
			for (uint i = 0; i < tableCount; i++) {
				uint rowSize;
				uint rowCount;

				ThrowOnError(_metaDataTables.GetTableInfo(i, out rowSize, out rowCount, out _, out _, out _));
				if (rowCount == 0)
					continue;
				tablesSize += rowSize * rowCount;
				validTableCount++;
			}
			headerSize = 0x18 + validTableCount * 4;
			ThrowOnError(_metaDataTables.GetRow(0, 1, out address));
			address = (byte*)address - headerSize;
			size = AlignUp(headerSize + tablesSize, 4);
			return new MetadataStreamInfo(address, size);
		}

		private MetadataStreamInfo GetStringHeap() {
			int result;
			uint streamSize;
			void* pData;

			ThrowOnError(_metaDataTables.GetStringHeapSize(out streamSize));
			if (streamSize == 1)
				// 表示流不存在，1只是用来占位
				return null;
			result = _metaDataTables.GetString(0, out pData);
			return result == 0 ? new MetadataStreamInfo(pData, AlignUp(streamSize, 4)) : null;
		}

		private MetadataStreamInfo GetUserStringHeap() {
			int result;
			uint streamSize;
			uint dataSize;
			void* pData;

			ThrowOnError(_metaDataTables.GetUserStringHeapSize(out streamSize));
			if (streamSize == 1)
				return null;
			streamSize = AlignUp(streamSize, 4);
			result = _metaDataTables.GetUserString(1, out dataSize, out pData);
			// #US与#Blob堆传入ixXXX=0都会导致获取到的pData不是真实地址，所以获取第2个数据的地址
			return result == 0 ? new MetadataStreamInfo((byte*)pData - GetCompressedUInt32Length(dataSize) - 1, streamSize) : null;
		}

		private MetadataStreamInfo GetGuidHeap() {
			int result;
			uint streamSize;
			void* pData;

			ThrowOnError(_metaDataTables.GetGuidHeapSize(out streamSize));
			if (streamSize == 1)
				return null;
			streamSize = AlignUp(streamSize, 4);
			result = _metaDataTables.GetGuid(1, out pData);
			return result == 0 ? new MetadataStreamInfo(pData, streamSize) : null;
		}

		private MetadataStreamInfo GetBlobHeap() {
			int result;
			uint streamSize;
			uint dataSize;
			void* pData;

			ThrowOnError(_metaDataTables.GetBlobHeapSize(out streamSize));
			if (streamSize == 1)
				return null;
			streamSize = AlignUp(streamSize, 4);
			result = _metaDataTables.GetBlob(1, out dataSize, out pData);
			return result == 0 ? new MetadataStreamInfo((byte*)pData - GetCompressedUInt32Length(dataSize) - 1, streamSize) : null;
		}

		private static uint AlignUp(uint value, uint alignment) {
			return (value + alignment - 1) & ~(alignment - 1);
		}

		private static byte GetCompressedUInt32Length(uint value) {
			if (value < 0x80)
				return 1;
			else if (value < 0x4000)
				return 2;
			else
				return 4;
		}

		private static void ThrowOnError(int result) {
			if (result != 0)
				throw new InvalidOperationException();
		}
	}
}
