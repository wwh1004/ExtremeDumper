using System.Collections.Generic;
using System.Diagnostics;
using ExtremeDumper.AntiAntiDump;

namespace ExtremeDumper.Diagnostics;

sealed class AADModulesProvider : IModulesProvider {
	readonly uint processId;

	public AADModulesProvider(uint processId) {
		this.processId = processId;
	}

	public IEnumerable<ModuleInfo> EnumerateModules() {
		foreach (var client in AADExtensions.EnumerateAADClients(processId)) {
			var runtime = client.Runtime;
			foreach (var module in client.EnumerateModules()) {
				var peInfo = client.GetPEInfo(module);
				if (peInfo.IsInvalid) {
					// may be ngen image and corresponding IL image not loaded TODO: get native image, not IL image
					yield return new DotNetModuleInfo(module.AssemblyName, unchecked((nuint)(-1)), 0, "NGEN", module.DomainName, $"v{runtime.FileVersion}");
					continue;
				}

				var layout = peInfo.LoadedLayout;
				Debug2.Assert(!layout.IsInvalid);
				yield return new DotNetModuleInfo(module.AssemblyName, (nuint)layout.ImageBase, layout.ImageSize, peInfo.FilePath, module.DomainName, $"v{runtime.FileVersion}");
			}
		}
	}
}
