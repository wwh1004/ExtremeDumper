using System;
using System.Diagnostics.CodeAnalysis;

namespace ExtremeDumper.AntiAntiDump;

/// <summary>
/// <see cref="AADCommand"/> handler interface
/// </summary>
/// <remarks>
/// Phase1 invoke command: Client.Invoke -> ICommandHandler.Invoke -> Server.Execute -> ICommandHandler.Execute -> result(in server)
/// Phase2 receive result: result(in server) -> ICommandHandler.Execute -> Server.Execute -> ICommandHandler.Invoke -> Client.Invoke
/// </remarks>
interface ICommandHandler {
	/// <summary>
	/// Registered command enum
	/// </summary>
	AADCommand Command { get; }

	/// <summary>
	/// Parameters type (server side)
	/// </summary>
	Type ParametersType { get; }

	/// <summary>
	/// Result type (client side)
	/// </summary>
	Type ResultType { get; }

	/// <summary>
	/// Execute command (server side)
	/// </summary>
	/// <param name="parameters"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	bool Execute(ISerializable parameters, [NotNullWhen(true)] out ISerializable? result);
}
