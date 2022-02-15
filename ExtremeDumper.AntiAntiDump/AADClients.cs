using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// <see cref="AADClient"/> aggregator used for <see cref="AADCommand.EnableMultiDomain"/>
/// </summary>
public sealed class AADClients : IEnumerable<AADClient>, IDisposable {
	readonly List<AADClient> clients = new();

	/// <summary>
	/// Are all client connected
	/// </summary>
	public bool IsConnected {
		get {
			if (clients.Count == 0)
				return false;
			foreach (var client in clients) {
				if (!client.IsConnected)
					return false;
			}
			return true;
		}
	}

	/// <summary>
	/// Current runtime info
	/// </summary>
	public RuntimeInfo Runtime => clients.Count != 0 ? clients[0].Runtime : throw new InvalidOperationException();

	/// <summary>
	/// All application domain infos
	/// </summary>
	public IEnumerable<AppDomainInfo> Domains {
		get {
			foreach (var client in clients)
				yield return client.Domain;
		}
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="client"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public AADClients(AADClient client) {
		if (client is null)
			throw new ArgumentNullException(nameof(client));

		clients.Add(client);
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="clients"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public AADClients(IEnumerable<AADClient> clients) {
		if (clients is null)
			throw new ArgumentNullException(nameof(clients));

		this.clients.AddRange(clients);
		Debug2.Assert(this.clients.Count != 0);
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

		clients.Add(mainClient);
		clients.AddRange(otherClients);
	}

	/// <summary>
	/// Use <paramref name="mainClient"/> as trampoline to enable multi domain mode
	/// </summary>
	/// <param name="mainClient"></param>
	/// <returns></returns>
	public static AADClients AsMultiDomain(AADClient mainClient) {
		if (mainClient is null)
			throw new ArgumentNullException(nameof(mainClient));

		Debug2.Assert(mainClient.IsConnected);
		if (mainClient.Runtime.Flavor != RuntimeFlavor.Framework)
			return new AADClients(mainClient);
		if (!mainClient.EnableMultiDomain(out var otherClients))
			throw new InvalidOperationException("Can't enable multi application domains mode");
		return new AADClients(mainClient, otherClients);
	}

	/// <summary>
	/// Call <see cref="AADClient.Connect"/> for all <see cref="AADClient"/>s
	/// </summary>
	/// <param name="timeout"></param>
	public bool ConnectAll(int timeout) {
		if (clients.Count == 0)
			return false;
		foreach (var client in clients) {
			if (!client.Connect(timeout))
				return false;
		}
		return true;
	}

	/// <summary>
	/// Call <see cref="AADClient.Disconnect"/> for all <see cref="AADClient"/>s
	/// </summary>
	public void DisconnectAll() {
		Dispose();
	}

	/// <summary>
	/// Get all modules
	/// </summary>
	/// <param name="modules"></param>
	/// <returns></returns>
	public bool GetModules([NotNullWhen(true)] out ModuleInfos? modules) {
		modules = null;
		var buffer = new ModuleInfos();
		foreach (var client in clients) {
			if (!client.GetModules(out var t))
				return false;
			buffer.AddRange(t);
		}
		modules = buffer;
		return true;
	}

	/// <summary>
	/// Get metadata of <paramref name="module"/>
	/// </summary>
	/// <param name="module"></param>
	/// <param name="metadataInfo"></param>
	/// <returns></returns>
	public bool GetMetadataInfo(ModuleInfo module, [NotNullWhen(true)] out MetadataInfo? metadataInfo) {
		metadataInfo = null;
		var client = FindClient(module.DomainId);
		return client is not null && client.GetMetadataInfo(module, out metadataInfo);
	}

	/// <summary>
	/// Get PE info of <paramref name="module"/>
	/// </summary>
	/// <param name="module"></param>
	/// <param name="peInfo"></param>
	/// <returns></returns>
	public bool GetPEInfo(ModuleInfo module, [NotNullWhen(true)] out PEInfo? peInfo) {
		peInfo = null;
		var client = FindClient(module.DomainId);
		return client is not null && client.GetPEInfo(module, out peInfo);
	}

	AADClient? FindClient(uint domainId) {
		if (clients.Count == 0)
			return null;
		foreach (var client in clients) {
			Debug2.Assert(client.IsConnected);
			if (client.Domain.Id == domainId)
				return client;
		}
		Debug2.Assert(false);
		return null;
	}

	/// <inheritdoc/>
	public IEnumerator<AADClient> GetEnumerator() {
		return clients.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return ((IEnumerable)clients).GetEnumerator();
	}

	/// <inheritdoc/>
	public void Dispose() {
		foreach (var client in clients)
			client.Dispose();
	}
}
