using System.Collections.Generic;
using System.IO;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// Module info
/// </summary>
public sealed class ModuleInfo : ISerializable {
	/// <summary>
	/// Module name
	/// </summary>
	public string Name = string.Empty;

	/// <summary>
	/// Owner assembly name
	/// </summary>
	public string AssemblyName = string.Empty;

	/// <summary>
	/// Owner application domain id
	/// </summary>
	public uint DomainId;

	/// <summary>
	/// Owner application domain name
	/// </summary>
	public string DomainName = string.Empty;

	bool ISerializable.Serialize(Stream destination) {
		return SimpleSerializer.Write(destination, this);
	}

	bool ISerializable.Deserialize(Stream source) {
		return SimpleSerializer.Read(source, this);
	}
}

/// <summary>
/// Module info list
/// </summary>
public sealed class ModuleInfos : List<ModuleInfo>, ISerializable {
	bool ISerializable.Serialize(Stream destination) {
		return SimpleSerializer.WriteList(destination, this);
	}

	bool ISerializable.Deserialize(Stream source) {
		return SimpleSerializer.ReadList(source, this);
	}
}
