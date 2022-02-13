using System;
using ExtremeDumper.AntiAntiDump;

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
			throw new InvalidOperationException($"Please call '{nameof(CreateWithAADClient)}' instead.");
		default:
			throw new ArgumentOutOfRangeException(nameof(type));
		}
	}

	/// <summary>
	/// Create managed module infos provider with <see cref="AADClient"/>
	/// </summary>
	/// <param name="client"></param>
	/// <returns></returns>
	public static IModulesProvider CreateWithAADClient(AADClient client) {
		if (client is null)
			throw new ArgumentNullException(nameof(client));

		throw new NotImplementedException();
	}
}
