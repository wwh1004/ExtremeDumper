using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ExtremeDumper.Diagnostics;
using ExtremeDumper.Injecting;
using ExtremeDumper.Logging;

namespace ExtremeDumper.AntiAntiDump;

static class SharedAADClientManager {
	static readonly Dictionary<uint, List<AADClients>> cache = new();
	static readonly object lockObj = new();

	public static List<AADClients> GetAADClients(uint processId) {
		return GetAADClients(processId, 1000);
	}

	public static List<AADClients> GetAADClients(uint processId, int timeout) {
		lock (lockObj)
			return GetAADClients_NoLock(processId, timeout);
	}

	static List<AADClients> GetAADClients_NoLock(uint processId, int timeout) {
		if (cache.TryGetValue(processId, out var clients)) {
			if (clients.First().First().IsConnected) {
				Debug2.Assert(clients.All(t => t.All(t => t.IsConnected)));
				return clients;
			}
			cache.Remove(processId);
			// process already exited, remove it from cache
		}

		var processInfo = DefaultProcessesProvider.GetProcessInfo(processId);
		if (processInfo is null)
			throw new InvalidOperationException("Can't get process info");
		if (processInfo is not DotNetProcessInfo dotNetProcessInfo)
			throw new InvalidOperationException("Target process is not a valid .NET process");

		clients = new List<AADClients>();
		if (dotNetProcessInfo.HasCLR2)
			clients.Add(SetupAADClients(processId, timeout, InjectionClrVersion.V2));
		if (dotNetProcessInfo.HasCLR4)
			clients.Add(SetupAADClients(processId, timeout, InjectionClrVersion.V4));
		if (dotNetProcessInfo.HasCoreCLR)
			Logger.Warning("Currently AntiAntiDump mode doesn't support CoreCLR");
		cache.Add(processId, clients);
		return clients;
	}

	static AADClients SetupAADClients(uint processId, int timeout, InjectionClrVersion clrVersion) {
		var mainClient = AADCoreInjector.Inject(processId, clrVersion);
		if (!mainClient.Connect(timeout))
			throw new InvalidOperationException("Can't connect to AADServer.");
		var clients = AADClients.AsMultiDomain(mainClient);
		if (!clients.ConnectAll(timeout))
			throw new InvalidOperationException("Can't connect to AADServer in other application domain.");
		return clients;
	}
}
