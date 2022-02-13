using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AAD = ExtremeDumper.AntiAntiDump;

namespace ExtremeDumper.Diagnostics;

sealed class AADModulesProvider : IModulesProvider {
	static readonly string coreFileName = Path.GetFileName(AAD.AADCoreInjector.GetAADCorePath());
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
				if (IsAntiAntiDumpModule(module))
					continue;

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

	static bool IsAntiAntiDumpModule(AAD.ModuleInfo module) {
		switch (module.Name) {
		case "00000000.dll":
		case "00000001.dll":
		case "00000002.dll":
		case "00000003.dll":
		case "00000004.dll":
		case "00000100.dll":
		case "00000101.dll":
		case "00000102.dll":
		case "00000103.dll":
		case "00000104.dll":
		case "00000200.dll":
		case "00000201.dll":
		case "00000202.dll":
		case "00000203.dll":
		case "00000204.dll":
		case "00000300.dll":
		case "00000301.dll":
		case "00000302.dll":
		case "00000303.dll":
		case "00000304.dll":
			return true;
		default:
			return module.Name == coreFileName;
		}
	}
}
