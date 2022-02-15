using System;
using System.Collections.Generic;
using System.Diagnostics;
using AAD = ExtremeDumper.AntiAntiDump;

namespace ExtremeDumper.Diagnostics;

sealed class AADModulesProvider : IModulesProvider {
	readonly List<AAD.AADClients> clientsList;

	public AADModulesProvider(AAD.AADClient client) : this(new AAD.AADClients(client)) {
	}

	public AADModulesProvider(AAD.AADClients clients) {
		if (clients is null)
			throw new ArgumentNullException(nameof(clients));

		clientsList = new List<AAD.AADClients> { clients };
	}

	public AADModulesProvider(IEnumerable<AAD.AADClients> clientsList) {
		if (clientsList is null)
			throw new ArgumentNullException(nameof(clientsList));

		this.clientsList = new List<AAD.AADClients>(clientsList);
	}

	public IEnumerable<ModuleInfo> EnumerateModules() {
		foreach (var clients in clientsList) {
			Debug2.Assert(clients.IsConnected);
			if (!clients.GetModules(out var modules))
				throw new InvalidOperationException("Can't get modules");

			var runtime = clients.Runtime;
			foreach (var module in modules) {
				if (!clients.GetPEInfo(module, out var peInfo))
					throw new InvalidOperationException("Can't get PE info");

				if (peInfo.IsInvalid) {
					// may be ngen image and corresponding IL image not loaded TODO: get native image, not IL image
					yield return new DotNetModuleInfo(module.AssemblyName, unchecked((nuint)(-1)), 0, "NGEN", module.DomainName, $"v{runtime.FileVersion}");
					continue;
				}
				// TODO: get clr version

				var layout = peInfo.LoadedLayout;
				Debug2.Assert(!layout.IsInvalid);
				yield return new DotNetModuleInfo(module.AssemblyName, (nuint)layout.ImageBase, layout.ImageSize, peInfo.FilePath, module.DomainName, $"v{runtime.FileVersion}");
			}
		}
	}
}
