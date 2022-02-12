using System;
using System.Diagnostics;
using System.IO;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// CLR internal image layout
/// </summary>
public sealed class PEImageLayout : ISerializable {
	/// <summary>
	/// Empty instance
	/// </summary>
	public static readonly PEImageLayout Empty = new();

	/// <summary>
	/// Image base address
	/// </summary>
	public ulong ImageBase;

	/// <summary>
	/// Image size (size of file on disk, as opposed to OptionalHeaders.SizeOfImage)
	/// </summary>
	public uint ImageSize;

	/// <summary>
	/// RVA of <see cref="MetadataLocator.RuntimeDefinitions.IMAGE_COR20_HEADER"/>
	/// </summary>
	public uint CorHeaderRVA;

	/// <summary>
	/// Populate data from <see cref="MetadataLocator.PEImageLayout"/>
	/// </summary>
	/// <param name="imageLayout"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public void Populate(MetadataLocator.PEImageLayout imageLayout) {
		if (imageLayout is null)
			throw new ArgumentNullException(nameof(imageLayout));

		ImageBase = imageLayout.ImageBase;
		ImageSize = imageLayout.ImageSize;
		CorHeaderRVA = (uint)(imageLayout.CorHeaderAddress - imageLayout.ImageBase);
	}

	bool ISerializable.Serialize(Stream destination) {
		return SimpleSerializer.Write(destination, this);
	}

	bool ISerializable.Deserialize(Stream source) {
		return SimpleSerializer.Read(source, this);
	}
}

/// <summary>
/// .NET PE Info
/// </summary>
public sealed class PEInfo : ISerializable {
	/// <summary>
	/// Empty instance
	/// </summary>
	public static readonly PEInfo Empty = new();

	/// <summary>
	/// Image file path
	/// </summary>
	public string FilePath = string.Empty;

	/// <summary>
	/// If image is loaded in memory
	/// </summary>
	public bool InMemory => string.IsNullOrEmpty(FilePath);

	/// <summary>
	/// Flat image layout, maybe empty (Assembly.Load(byte[]))
	/// </summary>
	public PEImageLayout FlatLayout = PEImageLayout.Empty;

	/// <summary>
	/// Mapped image layout, maybe empty (Assembly.LoadFile)
	/// </summary>
	public PEImageLayout MappedLayout = PEImageLayout.Empty;

	/// <summary>
	/// Loaded image layout, not empty (Assembly.LoadFile)
	/// </summary>
	public PEImageLayout LoadedLayout = PEImageLayout.Empty;

	/// <summary>
	/// Populate data from <see cref="MetadataLocator.PEInfo"/>
	/// </summary>
	/// <param name="peInfo"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public void Populate(MetadataLocator.PEInfo peInfo) {
		if (peInfo is null)
			throw new ArgumentNullException(nameof(peInfo));
		if (peInfo.IsInvalid)
			throw new ArgumentException($"Invalid {nameof(peInfo)}");

		FilePath = peInfo.FilePath;
		FlatLayout.Populate(peInfo.FlatLayout);
		MappedLayout.Populate(peInfo.MappedLayout);
		LoadedLayout.Populate(peInfo.LoadedLayout);
	}

	bool ISerializable.Serialize(Stream destination) {
		return SimpleSerializer.Write(destination, this);
	}

	bool ISerializable.Deserialize(Stream source) {
		return SimpleSerializer.Read(source, this);
	}
}

/// <summary>
/// Metadata schema
/// </summary>
public sealed class MetadataSchema : ISerializable {
	/// <summary>
	/// Empty instance
	/// </summary>
	public static readonly MetadataSchema Empty = new();

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
	/// Populate data from <see cref="MetadataLocator.MetadataSchema"/>
	/// </summary>
	/// <param name="schema"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public void Populate(MetadataLocator.MetadataSchema schema) {
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
	/// RVA of stream
	/// </summary>
	public uint RVA;

	/// <summary>
	/// Length of stream
	/// </summary>
	public uint Length;

	/// <summary>
	/// Populate data from <see cref="MetadataLocator.MetadataStreamInfo"/>
	/// </summary>
	/// <param name="stream"></param>
	/// <param name="imageBase"></param>
	/// <exception cref="ArgumentNullException"></exception>
	protected void Populate(MetadataLocator.MetadataStreamInfo stream, nuint imageBase) {
		if (stream is null)
			throw new ArgumentNullException(nameof(stream));

		RVA = (uint)(stream.Address - imageBase);
		Debug2.Assert((int)RVA > 0);
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
	/// Empty instance
	/// </summary>
	public static readonly MetadataTableInfo Empty = new();

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
	/// Populate data from <see cref="MetadataLocator.MetadataTableInfo"/>
	/// </summary>
	/// <param name="table"></param>
	/// <param name="imageBase"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public void Populate(MetadataLocator.MetadataTableInfo table, nuint imageBase) {
		if (table is null)
			throw new ArgumentNullException(nameof(table));

		Populate((MetadataLocator.MetadataStreamInfo)table, imageBase);
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
	/// Empty instance
	/// </summary>
	public static readonly MetadataHeapInfo Empty = new();

	/// <summary>
	/// Populate data from <see cref="MetadataLocator.MetadataHeapInfo"/>
	/// </summary>
	/// <param name="heap"></param>
	/// <param name="imageBase"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public void Populate(MetadataLocator.MetadataHeapInfo heap, nuint imageBase) {
		if (heap is null)
			throw new ArgumentNullException(nameof(heap));

		Populate((MetadataLocator.MetadataStreamInfo)heap, imageBase);
	}
}

/// <summary>
/// Metadata info
/// </summary>
public sealed class MetadataInfo : ISerializable {
	/// <summary>
	/// Empty instance
	/// </summary>
	public static readonly MetadataInfo Empty = new();

	/// <summary>
	/// PEInfo
	/// </summary>
	public PEInfo PEInfo = PEInfo.Empty;

	/// <summary>
	/// RVA of metadata
	/// </summary>
	public uint MetadataRVA;

	/// <summary>
	/// Size of metadata
	/// </summary>
	/// <remarks>Currently return 0x1c if table stream is uncompressed (aka #-)</remarks>
	public uint MetadataSize;

	/// <summary>
	/// Metadata schema
	/// </summary>
	public MetadataSchema Schema = MetadataSchema.Empty;

	/// <summary>
	/// #~ or #- info
	/// </summary>
	public MetadataTableInfo TableStream = MetadataTableInfo.Empty;

	/// <summary>
	/// #Strings heap info
	/// </summary>
	public MetadataHeapInfo StringHeap = MetadataHeapInfo.Empty;

	/// <summary>
	/// #US heap info
	/// </summary>
	public MetadataHeapInfo UserStringHeap = MetadataHeapInfo.Empty;

	/// <summary>
	/// #GUID heap info
	/// </summary>
	public MetadataHeapInfo GuidHeap = MetadataHeapInfo.Empty;

	/// <summary>
	/// #Blob heap info
	/// </summary>
	public MetadataHeapInfo BlobHeap = MetadataHeapInfo.Empty;

	/// <summary>
	/// Populate data from <see cref="MetadataLocator.MetadataInfo"/>
	/// </summary>
	/// <param name="metadata"></param>
	/// <param name="peInfo"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public void Populate(MetadataLocator.MetadataInfo metadata, MetadataLocator.PEInfo peInfo) {
		if (metadata is null)
			throw new ArgumentNullException(nameof(metadata));
		if (peInfo is null)
			throw new ArgumentNullException(nameof(peInfo));
		if (metadata.IsInvalid)
			throw new ArgumentException($"Invalid {nameof(metadata)}");
		if (peInfo.IsInvalid)
			throw new ArgumentException($"Invalid {nameof(peInfo)}");

		var imageLayout = FindMetadataImageLayout(peInfo, metadata.MetadataAddress);
		if (imageLayout is null)
			throw new InvalidOperationException("Can't find the PEImageLayout where the metadata is located");
		PEInfo.Populate(peInfo);
		MetadataRVA = (uint)(metadata.MetadataAddress - imageLayout.ImageBase);
		Debug2.Assert((int)MetadataRVA > 0);
		MetadataSize = metadata.MetadataSize;
		Schema.Populate(metadata.Schema);
		TableStream.Populate(metadata.TableStream, imageLayout.ImageBase);
		StringHeap.Populate(metadata.StringHeap, imageLayout.ImageBase);
		UserStringHeap.Populate(metadata.UserStringHeap, imageLayout.ImageBase);
		GuidHeap.Populate(metadata.GuidHeap, imageLayout.ImageBase);
		BlobHeap.Populate(metadata.BlobHeap, imageLayout.ImageBase);
	}

	static MetadataLocator.PEImageLayout? FindMetadataImageLayout(MetadataLocator.PEInfo peInfo, nuint metadataAddress) {
		if (Check(peInfo.FlatLayout, metadataAddress))
			return peInfo.FlatLayout;
		if (Check(peInfo.MappedLayout, metadataAddress))
			return peInfo.MappedLayout;
		if (Check(peInfo.LoadedLayout, metadataAddress))
			return peInfo.LoadedLayout;
		return null;

		static bool Check(MetadataLocator.PEImageLayout imageLayout, nuint metadataAddress) {
			if (imageLayout.IsInvalid)
				return false;
			return imageLayout.ImageBase <= metadataAddress && metadataAddress < imageLayout.ImageBase + imageLayout.ImageSize;
		}
	}

	bool ISerializable.Serialize(Stream destination) {
		return SimpleSerializer.Write(destination, this);
	}

	bool ISerializable.Deserialize(Stream source) {
		return SimpleSerializer.Read(source, this);
	}
}
