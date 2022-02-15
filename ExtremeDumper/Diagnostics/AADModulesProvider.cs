using System;
using System.Collections.Generic;
using System.Diagnostics;
using AAD = ExtremeDumper.AntiAntiDump;

namespace ExtremeDumper.Diagnostics;

sealed class AADModulesProvider : IModulesProvider {
	readonly AAD.AADClients clients;

	public AADModulesProvider(AAD.AADClient client) {
		if (client is null)
			throw new ArgumentNullException(nameof(client));

		clients = new AAD.AADClients(client);
	}

	public AADModulesProvider(AAD.AADClients clients) {
		if (clients is null)
			throw new ArgumentNullException(nameof(clients));

		this.clients = clients;
	}

	public IEnumerable<ModuleInfo> EnumerateModules() {
		foreach (var client in clients) {
			Debug2.Assert(client.IsConnected);
			if (!client.GetModules(out var modules))
				throw new InvalidOperationException("Can't get modules");

			foreach (var module in modules) {
				if (!clients.GetPEInfo(module, out var peInfo))
					throw new InvalidOperationException("Can't get PE info");

				if (peInfo.IsInvalid) {
					// may be ngen image and corresponding IL image not loaded TODO: get native image, not IL image
					yield return new DotNetModuleInfo(module.AssemblyName, unchecked((nuint)(-1)), 0, "@TODO", module.DomainName, "@TODO");
					continue;
				}
				// TODO: get clr version

				var layout = peInfo.LoadedLayout;
				Debug2.Assert(!layout.IsInvalid);
				yield return new DotNetModuleInfo(module.AssemblyName, (nuint)layout.ImageBase, layout.ImageSize, peInfo.FilePath, module.DomainName, "@TODO");
			}
		}
	}
}
