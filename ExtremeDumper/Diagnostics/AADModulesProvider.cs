using System;
using System.Collections.Generic;
using ExtremeDumper.AntiAntiDump;

namespace ExtremeDumper.Diagnostics;

sealed class AADModulesProvider : IModulesProvider {
	readonly AADClientAggregator client;

	public AADModulesProvider(AADClient client) {
		if (client is null)
			throw new ArgumentNullException(nameof(client));

		this.client = new AADClientAggregator(client);
	}

	public AADModulesProvider(AADClientAggregator client) {
		if (client is null)
			throw new ArgumentNullException(nameof(client));

		this.client = client;
	}

	public IEnumerable<ModuleInfo> EnumerateModules() {
		throw new NotImplementedException();
	}
}
