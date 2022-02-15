using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipes;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// Anti anti dump client
/// </summary>
public sealed class AADClient : AADPipe {
	AADClient[]? multiDomainClients;
	AADClient? mainClient;
	RuntimeInfo? runtimeInfoCache;
	AppDomainInfo? domainInfoCache;

	/// <summary>
	/// Current runtime info
	/// </summary>
	public RuntimeInfo Runtime {
		get {
			if (!GetRuntimeInfo(out var result))
				throw new InvalidOperationException();
			return result;
		}
	}

	/// <summary>
	/// Current application domain info
	/// </summary>
	public AppDomainInfo Domain {
		get {
			if (!GetAppDomainInfo(out var result))
				throw new InvalidOperationException();
			return result;
		}
	}

	AADClient(NamedPipeClientStream stream, AADClient? mainClient) : base(stream) {
		this.mainClient = mainClient;
	}

	/// <summary>
	/// Create a anti anti dump client
	/// </summary>
	/// <param name="pipeName">Name of named pipe stream</param>
	/// <returns></returns>
	public static AADClient? Create(string pipeName) {
		return Create(pipeName, null);
	}

	static AADClient? Create(string pipeName, AADClient? mainClient) {
		if (string.IsNullOrEmpty(pipeName))
			return null;

		try {
			var stream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
			return new AADClient(stream, mainClient);
		}
		catch {
			return null;
		}
	}

	/// <summary>
	/// Connects to a waiting server with an infinite time-out value.
	/// </summary>
	/// <returns></returns>
	public bool Connect() {
		return Connect(-1);
	}

	/// <summary>
	/// Connects to a waiting server within the specified time-out period.
	/// </summary>
	/// <param name="timeout">The number of milliseconds to wait for the server to respond before the connection times out.</param>
	/// <returns></returns>
	public bool Connect(int timeout) {
		if (IsConnected)
			return true;
		try {
			var stream = (NamedPipeClientStream)base.stream;
			stream.Connect(timeout);
		}
		catch {
		}
		return IsConnected;
	}

	/// <summary>
	/// Disconnect from server
	/// </summary>
	public void Disconnect() {
		Dispose();
	}

	/// <summary>
	/// Create <see cref="AADServer"/>s in other application domains and get corresponding <see cref="AADClient"/>s
	/// </summary>
	/// <param name="clients"></param>
	/// <returns></returns>
	public bool EnableMultiDomain(out AADClient[] clients) {
		if (mainClient is not null) {
			Debug2.Assert(false, "do NOT call EnableMultiDomain in sub AADClient");
			clients = Array2.Empty<AADClient>();
			return false;
		}

		if (multiDomainClients is not null) {
			foreach (var client in multiDomainClients)
				Debug2.Assert(client.IsConnected);
			clients = multiDomainClients;
			return true;
		}

		clients = Array2.Empty<AADClient>();
		if (!Invoke<Handlers.EnableMultiDomainHandler.PipeNames>(AADCommand.EnableMultiDomain, EmptySerializable.Instance, out var pipeNames))
			return false;
		clients = new AADClient[pipeNames.Values.Length];
		for (int i = 0; i < clients.Length; i++) {
			var client = Create(pipeNames.Values[i], this);
			if (client is null)
				return false;
			clients[i] = client;
		}
		multiDomainClients = clients;
		return true;
	}

	/// <summary>
	/// Get runtime info
	/// </summary>
	/// <param name="runtimeInfo"></param>
	/// <returns></returns>
	public bool GetRuntimeInfo([NotNullWhen(true)] out RuntimeInfo? runtimeInfo) {
		if (mainClient is not null)
			return mainClient.GetRuntimeInfo(out runtimeInfo);

		runtimeInfo = null;
		if (runtimeInfoCache is null) {
			if (!Invoke(AADCommand.GetRuntimeInfo, EmptySerializable.Instance, out runtimeInfoCache))
				return false;
		}
		runtimeInfo = runtimeInfoCache;
		return true;
	}

	/// <summary>
	/// Get application domain info
	/// </summary>
	/// <param name="runtimeInfo"></param>
	/// <returns></returns>
	public bool GetAppDomainInfo([NotNullWhen(true)] out AppDomainInfo? domainInfo) {
		domainInfo = null;
		if (domainInfoCache is null) {
			if (!Invoke(AADCommand.GetAppDomainInfo, EmptySerializable.Instance, out domainInfoCache))
				return false;
		}
		domainInfo = domainInfoCache;
		return true;
	}

	/// <summary>
	/// Get all modules
	/// </summary>
	/// <param name="modules"></param>
	/// <returns></returns>
	public bool GetModules([NotNullWhen(true)] out ModuleInfos? modules) {
		return Invoke(AADCommand.GetModules, EmptySerializable.Instance, out modules);
	}

	/// <summary>
	/// Get metadata of <paramref name="module"/>
	/// </summary>
	/// <param name="module"></param>
	/// <param name="metadataInfo"></param>
	/// <returns></returns>
	public bool GetMetadataInfo(ModuleInfo module, [NotNullWhen(true)] out MetadataInfo? metadataInfo) {
		return Invoke(AADCommand.GetMetadataInfo, module, out metadataInfo);
	}

	/// <summary>
	/// Get PE info of <paramref name="module"/>
	/// </summary>
	/// <param name="module"></param>
	/// <param name="peInfo"></param>
	/// <returns></returns>
	public bool GetPEInfo(ModuleInfo module, [NotNullWhen(true)] out PEInfo? peInfo) {
		return Invoke(AADCommand.GetPEInfo, module, out peInfo);
	}

	bool Invoke<T>(AADCommand command, ISerializable parameters, [NotNullWhen(true)] out T? result) where T : class, ISerializable {
		result = null;
		if (!IsConnected)
			return false;
		var r = InvokeOne(command, parameters, out var result2);
		Debug2.Assert(r != ExecutionResult.UnknownCommand);
		// internal error, we should fix it
		switch (r) {
		case ExecutionResult.IOError:
		case ExecutionResult.Disconnect:
			Disconnect();
			return false;
		case ExecutionResult.Success:
			result = result2 as T;
			return result is not null;
		default:
			return false;
		}
	}

	/// <summary>
	/// Invoke one specified command with parameters
	/// </summary>
	/// <param name="command"></param>
	/// <param name="parameters"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	/// <remarks>See executor: <seealso cref="AADServer.ExecuteOne"/></remarks>
	ExecutionResult InvokeOne(AADCommand command, ISerializable parameters, out ISerializable? result) {
		result = null;
		try {
			if (!CommandHandlerManager.Handlers.TryGetValue(command, out var handler)) {
				Debug2.Assert(false);
				return ExecutionResult.UnknownCommand;
			}
			// phase 1: get the handler corresponding to the command

			WriteCommand(command);
			Write(parameters);
			// phase 2: write parameters to stream

			ReadCommand(out var resultCommand);
			if (resultCommand == AADCommand.UnhandledException) {
				var exception = new AADServerInvocationException();
				Read(exception);
				throw exception;
			}
			if (resultCommand != AADCommand.Success)
				return ExecutionResult.Failure;
			// phase 3: read execution result

			result = Activator.CreateInstance(handler.ResultType, true) as ISerializable;
			if (result is null)
				throw new InvalidOperationException("Can't create result object");
			// phase 4: create result instance

			Read(result);
			// phase 5: read result from stream

			return ExecutionResult.Success;
		}
		catch (AADServerInvocationException) {
			throw;
		}
		catch (Exception ex) {
			Debug2.Assert(ex is IOException);
			// regard all unhandable exceptions as IO error
			return ExecutionResult.IOError;
		}
	}
}
