using System;

namespace ExtremeDumper.Diagnostics;

/// <summary>
/// Module infos provider type
/// </summary>
public enum ModulesProviderType {
	Unmanaged,
	Managed,
	ManagedAAD
}

/// <summary>
/// Module infos provider factory
/// </summary>
public static class ModulesProviderFactory {
	/// <summary>
	/// Create
	/// </summary>
	/// <param name="processId"></param>
	/// <param name="type"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public static IModulesProvider Create(uint processId, ModulesProviderType type) {
		switch (type) {
		case ModulesProviderType.Unmanaged:
			return new UnmanagedModulesProvider(processId);
		case ModulesProviderType.Managed:
			return new ManagedModulesProvider(processId);
		case ModulesProviderType.ManagedAAD:
			return new AADModulesProvider(processId);
		default:
			throw new ArgumentOutOfRangeException(nameof(type));
		}
	}
}
