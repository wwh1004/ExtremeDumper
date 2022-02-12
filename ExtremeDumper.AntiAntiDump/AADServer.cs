using System;
using System.Diagnostics;
using System.Extensions;
using System.IO;
using System.IO.Pipes;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// <see cref="AADCommand.UnhandledException"/>
/// </summary>
public sealed class AADServerInvocationException : Exception, ISerializable {
	string message = string.Empty;

	/// <inheritdoc/>
	public override string Message => message;

	/// <inheritdoc/>
	public AADServerInvocationException() {
	}

	/// <inheritdoc/>
	public AADServerInvocationException(string message) {
		this.message = message;
	}

	bool ISerializable.Serialize(Stream destination) {
		return Serializer.WriteString(destination, message);
	}

	bool ISerializable.Deserialize(Stream source) {
		return Serializer.ReadString(source, out message);
	}
}

/// <summary>
/// Anti anti dump server, which provides module info and metadata info to bypass anti dump
/// </summary>
public sealed class AADServer : AADPipe {
	bool isListening;

	AADServer(NamedPipeServerStream stream) : base(stream) {
	}

	/// <summary>
	/// Create a metadata server
	/// </summary>
	/// <param name="pipeName">Name of named pipe stream</param>
	/// <returns></returns>
	public static AADServer? Create(string pipeName) {
		if (string.IsNullOrEmpty(pipeName))
			return null;
		if (File.Exists($@"\\.\pipe\{pipeName}"))
			return null;

		try {
			var stream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1);
			return new AADServer(stream);
		}
		catch {
			return null;
		}
	}

	/// <summary>
	/// Waits for a client to connect to this <see cref="AADServer" /> object.
	/// </summary>
	/// <returns></returns>
	bool WaitForConnection() {
		return WaitForConnection(-1);
	}

	/// <summary>
	/// Connects to a waiting server within the specified time-out period.
	/// </summary>
	/// <param name="timeout">The number of milliseconds to wait for the client to connect before the connection times out.</param>
	/// <returns></returns>
	bool WaitForConnection(int timeout) {
		// TODO: timeout
		Debug2.Assert(!IsConnected);
		var stream = (NamedPipeServerStream)base.stream;
		stream.WaitForConnection();
		return IsConnected;
	}

	/// <summary>
	/// Start listening
	/// </summary>
	public void Listen() {
		if (isListening)
			throw new InvalidOperationException("Already listening");

		isListening = true;
		try {
			while (true) {
				WaitForConnection();
				Execute();
			}
		}
		finally {
			isListening = false;
		}
	}

	/// <summary>
	/// Execute loop until disconnect
	/// </summary>
	/// <returns><see langword="true"/> if disconnected by command, <see langword="false"/> if client disconnects without sending command</returns>
	/// <remarks>Guarantee no exception thrown</remarks>
	bool Execute() {
		while (true) {
			var r = ExecuteOne();
			Debug2.Assert(r != ExecutionResult.UnknownCommand && r != ExecutionResult.UnhandledException);
			// internal error, we should fix it
			switch (r) {
			case ExecutionResult.UnhandledException:
			case ExecutionResult.IOError:
			case ExecutionResult.Disconnect:
				try {
					var stream = (NamedPipeServerStream)base.stream;
					stream.Disconnect();
				}
				catch {
				}
				return r == ExecutionResult.Disconnect;
			}
		}
	}

	/// <summary>
	/// Execute one command and return execution result
	/// </summary>
	/// <returns></returns>
	/// <remarks>See invoker: <seealso cref="AADClient.InvokeOne(AADCommand, ISerializable, out ISerializable?)"/></remarks>
	ExecutionResult ExecuteOne() {
		try {
			ReadCommand(out var command);

			if (command == AADCommand.Disconnect)
				return ExecutionResult.Disconnect;

			if (!CommandHandlerManager.Handlers.TryGetValue(command, out var handler)) {
				Debug2.Assert(false);
				return ExecutionResult.UnknownCommand;
			}
			// phase 1: get the handler corresponding to the command

			if (Activator.CreateInstance(handler.ParametersType, true) is not ISerializable parameters)
				throw new InvalidOperationException("Can't create parameters object");
			// phase 2: create parameters instance

			Read(parameters);
			// phase 3: read parameters from stream

			if (!handler.Execute(parameters, out var result)) {
				WriteCommand(AADCommand.Failure);
				return ExecutionResult.Failure;
			}
			// phase 4: execute command with parameters

			WriteCommand(AADCommand.Success);
			Write(result);
			// phase 5: write result to stream

			return ExecutionResult.Success;
		}
		catch (IOException) {
			return ExecutionResult.IOError;
		}
		catch (Exception ex) {
			WriteCommand(AADCommand.UnhandledException, false);
			Write(new AADServerInvocationException(ex.ToFullString()), false);
			OnUnhandledException(ex);
			return ExecutionResult.UnhandledException;
		}
	}
}
