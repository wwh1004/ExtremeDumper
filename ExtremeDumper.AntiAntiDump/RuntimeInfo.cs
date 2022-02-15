using System;
using System.IO;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// CLR Runtime flavor
/// </summary>
public enum RuntimeFlavor {
	/// <summary>
	/// .NET Framework 1.0 ~ 4.8
	/// </summary>
	Framework,

	/// <summary>
	/// .NET Core 1.0 ~ 3.1
	/// </summary>
	Core,

	/// <summary>
	/// .NET 5.0 +
	/// </summary>
	Net
}

/// <summary>
/// CLR Runtime info
/// </summary>
public sealed class RuntimeInfo : ISerializable {
	/// <summary>
	/// Runtime flavor
	/// </summary>
	public RuntimeFlavor Flavor {
		get => (RuntimeFlavor)_Flavor;
		set => _Flavor = (int)value;
	}

	/// <summary/>
	public int _Flavor;

	/// <summary>
	/// File name
	/// </summary>
	public string FileName = string.Empty;

	/// <summary>
	/// File version
	/// </summary>
	public Version FileVersion {
		get => new(_FileVersion);
		set => _FileVersion = value.ToString();
	}

	/// <summary/>
	public string _FileVersion = string.Empty;

	bool ISerializable.Serialize(Stream destination) {
		return SimpleSerializer.Write(destination, this);
	}

	bool ISerializable.Deserialize(Stream source) {
		return SimpleSerializer.Read(source, this);
	}
}
