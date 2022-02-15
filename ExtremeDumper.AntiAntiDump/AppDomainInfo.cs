using System.IO;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// Application domain info
/// </summary>
public sealed class AppDomainInfo : ISerializable {
	/// <summary>
	/// Domain ID
	/// </summary>
	public uint Id;

	/// <summary>
	/// Domain name
	/// </summary>
	public string Name = string.Empty;

	bool ISerializable.Serialize(Stream destination) {
		return SimpleSerializer.Write(destination, this);
	}

	bool ISerializable.Deserialize(Stream source) {
		return SimpleSerializer.Read(source, this);
	}
}
