#pragma warning disable CS1591
using System;
using System.Runtime.InteropServices;

namespace MetadataLocator {
	[Guid("D8F579AB-402D-4B8E-82D9-5D63B1065C68")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	internal unsafe interface IMetaDataTables {
		[PreserveSig]
		int GetStringHeapSize(out uint pcbStrings);

		[PreserveSig]
		int GetBlobHeapSize(out uint pcbBlobs);

		[PreserveSig]
		int GetGuidHeapSize(out uint pcbGuids);

		[PreserveSig]
		int GetUserStringHeapSize(out uint pcbBlobs);

		[PreserveSig]
		int GetNumTables(out uint pcTables);

		[PreserveSig]
		int GetTableIndex(uint token, out uint pixTbl);

		[PreserveSig]
		int GetTableInfo(uint ixTbl, out uint pcbRow, out uint pcRows, out uint pcCols, out uint piKey, out void* ppName);

		[PreserveSig]
		int GetColumnInfo(uint ixTbl, uint ixCol, out uint poCol, out uint pcbCol, out uint pType, out void* ppName);

		[PreserveSig]
		int GetCodedTokenInfo(uint ixCdTkn, out uint pcTokens, out void* ppTokens, out void* ppName);

		[PreserveSig]
		int GetRow(uint ixTbl, uint rid, out void* ppRow);

		[PreserveSig]
		int GetColumn(uint ixTbl, uint ixCol, uint rid, out uint pVal);

		[PreserveSig]
		int GetString(uint ixString, out void* ppString);

		[PreserveSig]
		int GetBlob(uint ixBlob, out uint pcbData, out void* ppData);

		[PreserveSig]
		int GetGuid(uint ixGuid, out void* ppGUID);

		[PreserveSig]
		int GetUserString(uint ixUserString, out uint pcbData, out void* ppData);

		[PreserveSig]
		int GetNextString(uint ixString, out uint pNext);

		[PreserveSig]
		int GetNextBlob(uint ixBlob, out uint pNext);

		[PreserveSig]
		int GetNextGuid(uint ixGuid, out uint pNext);

		[PreserveSig]
		int GetNextUserString(uint ixUserString, out uint pNext);
	}

	[Guid("BADB5F70-58DA-43A9-A1C6-D74819F19B15")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	internal unsafe interface IMetaDataTables2 : IMetaDataTables {

		[PreserveSig]
		int GetMetaDataStorage(out void* ppvMd, out uint pcbMd);

		[PreserveSig]
		int GetMetaDataStreamInfo(uint ix, out void* ppchName, out void* ppv, out uint pcb);
	}
}
#pragma warning restore CS1591
