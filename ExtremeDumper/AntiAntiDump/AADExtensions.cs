using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtremeDumper.AntiAntiDump;

static class AADExtensions {
	public static IEnumerable<AADClient> EnumerateAADClients(uint processId) {
		return SharedAADClientManager.GetAADClients(processId).SelectMany(t => t);
	}

	public static IEnumerable<ModuleInfo> EnumerateModules(this AADClient client) {
		return client.GetModules(out var modules) ? modules : throw new InvalidOperationException("Can't get modules");
	}

	public static MetadataInfo GetMetadataInfo(this AADClient client, ModuleInfo module) {
		return client.GetMetadataInfo(module, out var peInfo) ? peInfo : throw new InvalidOperationException("Can't get metadata info");
	}

	public static PEInfo GetPEInfo(this AADClient client, ModuleInfo module) {
		return client.GetPEInfo(module, out var peInfo) ? peInfo : throw new InvalidOperationException("Can't get PE info");
	}
}
