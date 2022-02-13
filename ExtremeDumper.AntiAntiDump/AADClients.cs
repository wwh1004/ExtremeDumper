using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// <see cref="AADClient"/> aggregator
/// </summary>
public sealed class AADClients : List<AADClient> {
	/// <summary>
	/// Are all client connected
	/// </summary>
	public bool IsConnected {
		get {
			if (Count == 0)
				return false;
			foreach (var client in this) {
				if (!client.IsConnected)
					return false;
			}
			return true;
		}
	}

	/// <summary>
	/// Constructor
	/// </summary>
	public AADClients() {
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="client"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public AADClients(AADClient client) {
		if (client is null)
			throw new ArgumentNullException(nameof(client));

		Add(client);
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="clients"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public AADClients(IEnumerable<AADClient> clients) {
		if (clients is null)
			throw new ArgumentNullException(nameof(clients));

		AddRange(clients);
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="mainClient"></param>
	/// <param name="otherClients"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public AADClients(AADClient mainClient, IEnumerable<AADClient> otherClients) {
		if (mainClient is null)
			throw new ArgumentNullException(nameof(mainClient));
		if (otherClients is null)
			throw new ArgumentNullException(nameof(otherClients));

		Add(mainClient);
		AddRange(otherClients);
	}

	/// <summary>
	/// Get all modules
	/// </summary>
	/// <param name="modules"></param>
	/// <returns></returns>
	public bool GetModules([NotNullWhen(true)] out ModuleInfos? modules) {
		modules = new ModuleInfos();
		foreach (var client in this) {
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
		if (Count == 0)
			return false;
		foreach (var client in this) {
			if (client.GetMetadata(module, out metadata))
				return true;
		}
		return false;
	}

	/// <summary>
	/// Get PE info of <paramref name="module"/>
	/// </summary>
	/// <param name="module"></param>
	/// <param name="peInfo"></param>
	/// <returns></returns>
	public bool GetPEInfo(ModuleInfo module, [NotNullWhen(true)] out PEInfo? peInfo) {
		peInfo = null;
		if (Count == 0)
			return false;
		foreach (var client in this) {
			if (client.GetPEInfo(module, out peInfo))
				return true;
		}
		return false;
	}

	/// <summary>
	/// Call <see cref="AADClient.Disconnect"/> for all <see cref="AADClient"/>
	/// </summary>
	public void DisconnectAll() {
		if (Count == 0)
			return;
		foreach (var client in this)
			client.Disconnect();
	}
}
