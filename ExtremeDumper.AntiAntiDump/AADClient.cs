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
	AADClient(NamedPipeClientStream stream) : base(stream) {
	}

	/// <summary>
	/// Create a metadata client
	/// </summary>
	/// <param name="pipeName">Name of named pipe stream</param>
	/// <returns></returns>
	public static AADClient? Create(string pipeName) {
		if (string.IsNullOrEmpty(pipeName))
			return null;

		try {
			var stream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
			return new AADClient(stream);
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
		Debug.Assert(!IsConnected);
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
	/// <param name="timeout">Timeout to wait <see cref="AADClient"/> connection</param>
	/// <param name="clients"></param>
	/// <returns></returns>
	public bool EnableMultiDomain(int timeout, out AADClient[] clients) {
		clients = Array2.Empty<AADClient>();
		if (!Invoke<Handlers.EnableMultiDomainHandler.PipeNames>(AADCommand.EnableMultiDomain, EmptySerializable.Instance, out var pipeNames))
			return false;
		clients = new AADClient[pipeNames.Values.Length];
		for (int i = 0; i < clients.Length; i++) {
			var client = Create(pipeNames.Values[i]);
			if (client is null)
				return false;
			if (!client.Connect(timeout))
				return false;
			clients[i] = client;
		}
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
	/// <param name="metadata"></param>
	/// <returns></returns>
	public bool GetMetadata(ModuleInfo module, [NotNullWhen(true)] out MetadataInfo? metadata) {
		return Invoke(AADCommand.GetMetadata, module, out metadata);
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
		Debug2.Assert(r != ExecutionResult.UnknownCommand && r != ExecutionResult.UnhandledException);
		// internal error, we should fix it
		switch (r) {
		case ExecutionResult.UnhandledException:
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
		catch (IOException) {
			return ExecutionResult.IOError;
		}
		catch (AADServerInvocationException) {
			throw;
		}
		catch (Exception ex) {
			Debug2.Assert(false);
			OnUnhandledException(ex);
			return ExecutionResult.UnhandledException;
		}
	}
}
