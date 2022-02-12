using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// <see cref="AADClient"/> aggregator
/// </summary>
public sealed class AADClientAggregator {
	readonly List<AADClient> clients = new();

	/// <summary>
	/// All aggregated <see cref="AADClient"/>
	/// </summary>
	public IEnumerable<AADClient> Clients => clients;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="mainClient"></param>
	/// <param name="otherClients"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public AADClientAggregator(AADClient mainClient, AADClient[] otherClients) {
		if (mainClient is null)
			throw new ArgumentNullException(nameof(mainClient));
		if (otherClients is null)
			throw new ArgumentNullException(nameof(otherClients));

		clients.Add(mainClient);
		clients.AddRange(otherClients);
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="clients"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public AADClientAggregator(IEnumerable<AADClient> clients) {
		if (clients is null)
			throw new ArgumentNullException(nameof(clients));

		this.clients.AddRange(clients);
	}

	/// <summary>
	/// Get all modules
	/// </summary>
	/// <param name="modules"></param>
	/// <returns></returns>
	public bool GetModules([NotNullWhen(true)] out ModuleInfos? modules) {
		modules = new ModuleInfos();
		foreach (var client in clients) {
			if (!client.GetModules(out var t))
				return false;
			modules.AddRange(t);
		}
		return true;
	}

	/// <summary>
	/// Get metadata of <paramref name="module"/>
	/// </summary>
	/// <param name="module"></param>
	/// <param name="metadata"></param>
	/// <returns></returns>
	public bool GetMetadata(ModuleInfo module, [NotNullWhen(true)] out MetadataInfo? metadata) {
		metadata = null;
		foreach (var client in clients) {
			if (client.GetMetadata(module, out metadata))
				return true;
		}
		return false;
	}

	/// <summary>
	/// Call <see cref="AADClient.Disconnect"/> for all <see cref="AADClient"/> in <see cref="Clients"/>
	/// </summary>
	public void DisconnectAll() {
		foreach (var client in clients)
			client.Disconnect();
	}
}
