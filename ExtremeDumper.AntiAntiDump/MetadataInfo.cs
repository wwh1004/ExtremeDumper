using System;
using System.IO;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// Metadata schema
/// </summary>
public sealed class MetadataSchema : ISerializable {
	/// <summary>
	/// Determine if current instance is invalid
	/// </summary>
	public bool IsInvalid => ValidMask == 0;

	/// <summary/>
	public uint Reserved1;

	/// <summary/>
	public byte MajorVersion;

	/// <summary/>
	public byte MinorVersion;

	/// <summary/>
	public byte Flags;

	/// <summary/>
	public byte Log2Rid;

	/// <summary/>
	public ulong ValidMask;

	/// <summary/>
	public ulong SortedMask;

	/// <summary/>
	/// <remarks>Array length always equals to <see cref="MetadataLocator.RuntimeDefinitions.TBL_COUNT"/> if not empty</remarks>
	public uint[] RowCounts = Array2.Empty<uint>();

	/// <summary/>
	public uint ExtraData;

	/// <summary>
	/// Default constructor
	/// </summary>
	public MetadataSchema() {
	}

	/// <summary>
	/// Populate data from <see cref="MetadataLocator.MetadataSchema"/>
	/// </summary>
	/// <param name="schema"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public MetadataSchema(MetadataLocator.MetadataSchema schema) {
		if (schema is null)
			throw new ArgumentNullException(nameof(schema));

		Reserved1 = schema.Reserved1;
		MajorVersion = schema.MajorVersion;
		MinorVersion = schema.MinorVersion;
		Flags = schema.Flags;
		Log2Rid = schema.Log2Rid;
		ValidMask = schema.ValidMask;
		SortedMask = schema.SortedMask;
		RowCounts = schema.RowCounts;
		ExtraData = schema.ExtraData;
	}

	bool ISerializable.Serialize(Stream destination) {
		return SimpleSerializer.Write(destination, this);
	}

	bool ISerializable.Deserialize(Stream source) {
		return SimpleSerializer.Read(source, this);
	}
}

/// <summary>
/// Metadata stream info
/// </summary>
public abstract class MetadataStreamInfo : ISerializable {
	/// <summary>
	/// Determine if current instance is invalid
	/// </summary>
	public bool IsInvalid => Address == 0;

	/// <summary>
	/// Address of stream
	/// </summary>
	public ulong Address;

	/// <summary>
	/// Length of stream
	/// </summary>
	public uint Length;

	/// <summary>
	/// Default constructor
	/// </summary>
	protected MetadataStreamInfo() {
	}

	/// <summary>
	/// Populate data from <see cref="MetadataLocator.MetadataStreamInfo"/>
	/// </summary>
	/// <param name="stream"></param>
	/// <exception cref="ArgumentNullException"></exception>
	protected MetadataStreamInfo(MetadataLocator.MetadataStreamInfo stream) {
		if (stream is null)
			throw new ArgumentNullException(nameof(stream));

		Address = stream.Address;
		Length = stream.Length;
	}

	bool ISerializable.Serialize(Stream destination) {
		return SimpleSerializer.Write(destination, this);
	}

	bool ISerializable.Deserialize(Stream source) {
		return SimpleSerializer.Read(source, this);
	}
}

/// <summary>
/// Metadata table info (#~, #-)
/// </summary>
public sealed class MetadataTableInfo : MetadataStreamInfo {
	/// <summary>
	/// Is compressed table stream (#~)
	/// </summary>
	public bool IsCompressed;

	/// <summary>
	/// Table count, see <see cref="MetadataLocator.RuntimeDefinitions.TBL_COUNT_V1"/> and  <see cref="MetadataLocator.RuntimeDefinitions.TBL_COUNT_V2"/>
	/// </summary>
	public uint TableCount;

	/// <summary>
	/// Size of each row
	/// </summary>
	/// <remarks>Array length always equals to <see cref="MetadataLocator.RuntimeDefinitions.TBL_COUNT"/> if not empty</remarks>
	public uint[] RowSizes = Array2.Empty<uint>();

	/// <summary>
	/// Default constructor
	/// </summary>
	public MetadataTableInfo() {
	}

	/// <summary>
	/// Populate data from <see cref="MetadataLocator.MetadataTableInfo"/>
	/// </summary>
	/// <param name="table"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public MetadataTableInfo(MetadataLocator.MetadataTableInfo table) : base(table) {
		if (table is null)
			throw new ArgumentNullException(nameof(table));

		IsCompressed = table.IsCompressed;
		TableCount = table.TableCount;
		RowSizes = table.RowSizes;
	}
}

/// <summary>
/// Metadata heap info (#Strings, #US, #GUID, #Blob)
/// </summary>
public sealed class MetadataHeapInfo : MetadataStreamInfo {
	/// <summary>
	/// Default constructor
	/// </summary>
	public MetadataHeapInfo() {
	}

	/// <summary>
	/// Populate data from <see cref="MetadataLocator.MetadataTableInfo"/>
	/// </summary>
	/// <param name="heap"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public MetadataHeapInfo(MetadataLocator.MetadataHeapInfo heap) : base(heap) {
	}
}

/// <summary>
/// Metadata info
/// </summary>
public sealed class MetadataInfo : ISerializable {
	/// <summary>
	/// Determine if current instance is invalid
	/// </summary>
	public bool IsInvalid => MetadataAddress == 0;

	/// <summary>
	/// Address of metadata
	/// </summary>
	public ulong MetadataAddress;

	/// <summary>
	/// Size of metadata
	/// </summary>
	/// <remarks>Currently return 0x1c if table stream is uncompressed (aka #-)</remarks>
	public uint MetadataSize;

	/// <summary>
	/// Metadata schema
	/// </summary>
	public MetadataSchema Schema = new();

	/// <summary>
	/// #~ or #- info
	/// </summary>
	public MetadataTableInfo TableStream = new();

	/// <summary>
	/// #Strings heap info
	/// </summary>
	public MetadataHeapInfo StringHeap = new();

	/// <summary>
	/// #US heap info
	/// </summary>
	public MetadataHeapInfo UserStringHeap = new();

	/// <summary>
	/// #GUID heap info
	/// </summary>
	public MetadataHeapInfo GuidHeap = new();

	/// <summary>
	/// #Blob heap info
	/// </summary>
	public MetadataHeapInfo BlobHeap = new();

	/// <summary>
	/// Default constructor
	/// </summary>
	public MetadataInfo() {
	}

	/// <summary>
	/// Populate data from <see cref="MetadataLocator.MetadataInfo"/>
	/// </summary>
	/// <param name="metadata"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public MetadataInfo(MetadataLocator.MetadataInfo metadata) {
		if (metadata is null)
			throw new ArgumentNullException(nameof(metadata));

		MetadataAddress = metadata.MetadataAddress;
		MetadataSize = metadata.MetadataSize;
		Schema = new MetadataSchema(metadata.Schema);
		TableStream = new MetadataTableInfo(metadata.TableStream);
		StringHeap = new MetadataHeapInfo(metadata.StringHeap);
		UserStringHeap = new MetadataHeapInfo(metadata.UserStringHeap);
		GuidHeap = new MetadataHeapInfo(metadata.GuidHeap);
		BlobHeap = new MetadataHeapInfo(metadata.BlobHeap);
	}

	bool ISerializable.Serialize(Stream destination) {
		return SimpleSerializer.Write(destination, this);
	}

	bool ISerializable.Deserialize(Stream source) {
		return SimpleSerializer.Read(source, this);
	}
}
