using System;
using System.IO;
using System.IO.Pipes;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// Anti anti dump server/client base class
/// </summary>
public abstract class AADPipe : IDisposable {
	internal enum ExecutionResult {
		Success,
		Failure,
		UnknownCommand,
		UnhandledException,
		IOError,
		Disconnect
	}

	internal readonly PipeStream stream;
	bool isDisposed;

	/// <summary>
	/// Triggered when internal exception is unhandled
	/// </summary>
	public event UnhandledExceptionEventHandler? UnhandledException;

	/// <summary>
	/// Is connected
	/// </summary>
	public bool IsConnected => stream.IsConnected;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="stream"></param>
	internal AADPipe(PipeStream stream) {
		this.stream = stream;
	}

	/// <summary>
	/// Read command from stream
	/// </summary>
	/// <param name="command"></param>
	/// <param name="throwing"></param>
	/// <returns></returns>
	/// <exception cref="IOException"></exception>
	internal bool ReadCommand(out AADCommand command, bool throwing = true) {
		command = AADCommand.Invalid;
		var buffer = new byte[4];
		if (!Read(buffer, throwing))
			return false;
		command = (AADCommand)BitConverter.ToUInt32(buffer, 0);
		return true;
	}

	/// <summary>
	/// Write command to stream
	/// </summary>
	/// <param name="command"></param>
	/// <returns></returns>
	/// <exception cref="IOException"></exception>
	internal bool WriteCommand(AADCommand command, bool throwing = true) {
		var buffer = BitConverter.GetBytes((uint)command);
		return Write(buffer, throwing);
	}

	/// <summary>
	/// Read serializable object from stream
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="throwing"></param>
	/// <returns></returns>
	/// <exception cref="IOException"></exception>
	internal bool Read(ISerializable obj, bool throwing = true) {
		bool b;
		try {
			b = obj.Deserialize(stream);
		}
		catch {
			b = false;
		}
		if (!b && throwing)
			throw new IOException("Can't read serializable object from internal pipe stream.");
		return b;
	}

	/// <summary>
	/// Write serializable object to stream
	/// </summary>
	/// <param name="obj"></param>
	/// <param name="throwing"></param>
	/// <returns></returns>
	/// <exception cref="IOException"></exception>
	internal bool Write(ISerializable obj, bool throwing = true) {
		bool b;
		try {
			b = obj.Serialize(stream);
		}
		catch {
			b = false;
		}
		if (!b && throwing)
			throw new IOException("Can't write serializable object to internal pipe stream.");
		return b;
	}

	/// <summary>
	/// Read bytes from stream 
	/// </summary>
	/// <param name="buffer"></param>
	/// <param name="throwing"></param>
	/// <returns></returns>
	/// <exception cref="IOException"></exception>
	internal bool Read(byte[] buffer, bool throwing = true) {
		bool b;
		try {
			b = stream.Read(buffer, 0, buffer.Length) == buffer.Length;
		}
		catch {
			b = false;
		}
		if (!b && throwing)
			throw new IOException("Can't read data from internal pipe stream.");
		return b;
	}

	/// <summary>
	/// Write bytes to stream
	/// </summary>
	/// <param name="buffer"></param>
	/// <param name="throwing"></param>
	/// <returns></returns>
	/// <exception cref="IOException"></exception>
	internal bool Write(byte[] buffer, bool throwing = true) {
		bool b;
		try {
			stream.Write(buffer, 0, buffer.Length);
			b = true;
		}
		catch {
			b = false;
		}
		if (!b && throwing)
			throw new IOException("Can't write data to internal pipe stream.");
		return b;
	}

	internal void OnUnhandledException(Exception exception) {
		var callback = UnhandledException;
		callback?.Invoke(this, new UnhandledExceptionEventArgs(exception, false));
	}

	/// <inheritdoc/>
	public void Dispose() {
		if (!isDisposed) {
			if (IsConnected)
				WriteCommand(AADCommand.Disconnect, false);
			stream.Dispose();
			isDisposed = true;
		}
	}
}
