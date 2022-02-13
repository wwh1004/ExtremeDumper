using System;
using System.IO;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// CLR internal image layout
/// </summary>
public sealed class PEImageLayout : ISerializable {
	/// <summary>
	/// Determine if current instance is invalid
	/// </summary>
	public bool IsInvalid => ImageBase == 0;

	/// <summary>
	/// Image base address
	/// </summary>
	public ulong ImageBase;

	/// <summary>
	/// Image size (size of file on disk, as opposed to OptionalHeaders.SizeOfImage)
	/// </summary>
	public uint ImageSize;

	/// <summary>
	/// Address of <see cref="MetadataLocator.RuntimeDefinitions.IMAGE_COR20_HEADER"/>
	/// </summary>
	public ulong CorHeaderAddress;

	/// <summary>
	/// Default constructor
	/// </summary>
	public PEImageLayout() {
	}

	/// <summary>
	/// Populate data from <see cref="MetadataLocator.PEImageLayout"/>
	/// </summary>
	/// <param name="imageLayout"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public PEImageLayout(MetadataLocator.PEImageLayout imageLayout) {
		if (imageLayout is null)
			throw new ArgumentNullException(nameof(imageLayout));

		ImageBase = imageLayout.ImageBase;
		ImageSize = imageLayout.ImageSize;
		CorHeaderAddress = imageLayout.CorHeaderAddress;
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
	/// Determine if current instance is invalid
	/// </summary>
	public bool IsInvalid => LoadedLayout.IsInvalid;

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
	public PEImageLayout FlatLayout = new();

	/// <summary>
	/// Mapped image layout, maybe empty (Assembly.LoadFile)
	/// </summary>
	public PEImageLayout MappedLayout = new();

	/// <summary>
	/// Loaded image layout, not empty (Assembly.LoadFile)
	/// </summary>
	public PEImageLayout LoadedLayout = new();

	/// <summary>
	/// Default constructor
	/// </summary>
	public PEInfo() {
	}

	/// <summary>
	/// Populate data from <see cref="MetadataLocator.PEInfo"/>
	/// </summary>
	/// <param name="peInfo"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public PEInfo(MetadataLocator.PEInfo peInfo) {
		if (peInfo is null)
			throw new ArgumentNullException(nameof(peInfo));

		FilePath = peInfo.FilePath;
		FlatLayout = new PEImageLayout(peInfo.FlatLayout);
		MappedLayout = new PEImageLayout(peInfo.MappedLayout);
		LoadedLayout = new PEImageLayout(peInfo.LoadedLayout);
	}

	bool ISerializable.Serialize(Stream destination) {
		return SimpleSerializer.Write(destination, this);
	}

	bool ISerializable.Deserialize(Stream source) {
		return SimpleSerializer.Read(source, this);
	}
}
