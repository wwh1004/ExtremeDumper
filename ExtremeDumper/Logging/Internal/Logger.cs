using System;

namespace Tool.Logging;

/// <summary>
/// Global logger
/// </summary>
public static partial class Logger {
	/// <summary>
	/// Gets current logger implement
	/// </summary>
	/// <returns></returns>
	public static ILogger Impl => ExternImpl ?? DefaultImpl;

	/// <summary>
	/// Gets default logger implement
	/// </summary>
	public static ILogger DefaultImpl => DefaultLogger.ConsoleOnlyInstance;

	/// <summary>
	/// Gets or sets customized logger
	/// </summary>
	public static ILogger? ExternImpl { get; set; }

	/// <summary>
	/// Indicates current log level, only logs of which log level greater than or equal to current log level will be logged
	/// </summary>
	public static LogLevel Level { get => Impl.Level; set => Impl.Level = value; }

	/// <summary>
	/// Indicates current logger in running in async mode
	/// </summary>
	public static bool IsAsync { get => Impl.IsAsync; set => Impl.IsAsync = value; }

	/// <summary>
	/// Indicates whether log queue is empty and background logger thread is idle (for async mode)
	/// </summary>
	public static bool IsIdle => Impl.IsIdle;

	/// <summary>
	/// Indicates current enqueued log count (for async mode)
	/// </summary>
	public static int QueueCount => Impl.QueueCount;

	/// <summary>
	/// Indicates whether current logger is locked. If locked, only who owners lock can access current logger
	/// </summary>
	public static bool IsLocked => Impl.IsLocked;

	/// <summary>
	/// Logs empty line
	/// </summary>
	public static void Info() { Impl.Info(); }

	/// <summary>
	/// Logs info and wraps
	/// </summary>
	/// <param name="value"></param>
	public static void Info(string? value) { Impl.Info(value); }

	/// <summary>
	/// Logs warning and wraps
	/// </summary>
	/// <param name="value"></param>
	public static void Warning(string? value) { Impl.Warning(value); }

	/// <summary>
	/// Logs error and wraps
	/// </summary>
	/// <param name="value"></param>
	public static void Error(string? value) { Impl.Error(value); }

	/// <summary>
	/// Logs level 1 verbose info and wraps
	/// </summary>
	/// <param name="value"></param>
	public static void Verbose1(string? value) { Impl.Verbose1(value); }

	/// <summary>
	/// Logs level 1 verbose info and wraps
	/// </summary>
	/// <param name="value"></param>
	public static void Verbose1(ref Verbose1InterpolatedStringHandler value) { var s = value.ToStringAndClear(); if (s is not null) Impl.Verbose1(s); }

	/// <summary>
	/// Logs level 2 verbose info and wraps
	/// </summary>
	/// <param name="value"></param>
	public static void Verbose2(string? value) { Impl.Verbose2(value); }

	/// <summary>
	/// Logs level 2 verbose info and wraps
	/// </summary>
	/// <param name="value"></param>
	public static void Verbose2(ref Verbose2InterpolatedStringHandler value) { var s = value.ToStringAndClear(); if (s is not null) Impl.Verbose2(s); }

	/// <summary>
	/// Logs level 3 verbose info and wraps
	/// </summary>
	/// <param name="value"></param>
	public static void Verbose3(string? value) { Impl.Verbose3(value); }

	/// <summary>
	/// Logs level 3 verbose info and wraps
	/// </summary>
	/// <param name="value"></param>
	public static void Verbose3(ref Verbose3InterpolatedStringHandler value) { var s = value.ToStringAndClear(); if (s is not null) Impl.Verbose3(s); }

	/// <summary>
	/// Logs exception and wraps
	/// </summary>
	/// <param name="value"></param>
	public static void Exception(Exception? value) { Impl.Exception(value); }

	/// <summary>
	/// Logs text with specified color and wraps
	/// </summary>
	/// <param name="value"></param>
	/// <param name="level"></param>
	/// <param name="color"></param>
	public static void Log(string? value, LogLevel level, ConsoleColor? color = null) { Impl.Log(value, level, color); }

	/// <summary>
	/// Immediately flushes buffer and waits to clear buffer (for async mode)
	/// </summary>
	public static void Flush() { Impl.Flush(); }

	/// <summary>
	/// Gets current logger with lock, current logger can be accessd only by the returned sub logger
	/// </summary>
	/// <returns></returns>
	public static ILogger EnterLock() { return Impl.EnterLock(); }

	/// <summary>
	/// Exits lock mode and returns parent logger
	/// </summary>
	/// <returns></returns>
	public static ILogger ExitLock() { return Impl.ExitLock(); }
}
