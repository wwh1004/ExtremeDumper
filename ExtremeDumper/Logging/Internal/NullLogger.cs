using System;

namespace Tool.Logging;

/// <summary>
/// Null logger implement which won't output any information
/// </summary>
public sealed class NullLogger : ILogger {
	/// <summary>
	/// Null logger instance
	/// </summary>
	public static ILogger Instance { get; } = new NullLogger();
	NullLogger() { }
	LogLevel ILogger.Level { get => LogLevel.Info; set { } }
	bool ILogger.IsAsync { get => false; set { } }
	bool ILogger.IsIdle => true;
	int ILogger.QueueCount => 0;
	bool ILogger.IsLocked => false;
	void ILogger.Info() { }
	void ILogger.Info(string? value) { }
	void ILogger.Warning(string? value) { }
	void ILogger.Error(string? value) { }
	void ILogger.Verbose1(string? value) { }
	void ILogger.Verbose2(string? value) { }
	void ILogger.Verbose3(string? value) { }
	void ILogger.Exception(Exception? value) { }
	void ILogger.Log(string? value, LogLevel level, ConsoleColor? color) { }
	void ILogger.Flush() { }
	ILogger ILogger.EnterLock() { return this; }
	ILogger ILogger.ExitLock() { return this; }
}
