using System;

namespace Tool.Logging;

/// <summary>
/// Log level
/// </summary>
public enum LogLevel {
	/// <summary>
	/// Error
	/// </summary>
	Error,

	/// <summary>
	/// Warning
	/// </summary>
	Warning,

	/// <summary>
	/// Information
	/// </summary>
	Info,

	/// <summary>
	/// Verbose info（Level 1）
	/// </summary>
	Verbose1,

	/// <summary>
	/// Verbose info（Level 2）
	/// </summary>
	Verbose2,

	/// <summary>
	/// Verbose info（Level 3）
	/// </summary>
	Verbose3
}

/// <summary>
/// Tool logger interface
/// </summary>
public interface ILogger {
	/// <summary>
	/// Indicates current log level, only logs of which log level greater than or equal to current log level will be logged
	/// </summary>
	LogLevel Level { get; set; }

	/// <summary>
	/// Indicates current logger in running in async mode
	/// </summary>
	bool IsAsync { get; set; }

	/// <summary>
	/// Indicates whether log queue is empty and background logger thread is idle (for async mode)
	/// </summary>
	bool IsIdle { get; }

	/// <summary>
	/// Indicates current enqueued log count (for async mode)
	/// </summary>
	int QueueCount { get; }

	/// <summary>
	/// Indicates whether current logger is locked. If locked, only who owners lock can access current logger
	/// </summary>
	bool IsLocked { get; }

	/// <summary>
	/// Logs empty line
	/// </summary>
	void Info();

	/// <summary>
	/// Logs info and wraps
	/// </summary>
	/// <param name="value"></param>
	void Info(string? value);

	/// <summary>
	/// Logs warning and wraps
	/// </summary>
	/// <param name="value"></param>
	void Warning(string? value);

	/// <summary>
	/// Logs error and wraps
	/// </summary>
	/// <param name="value"></param>
	void Error(string? value);

	/// <summary>
	/// Logs level 1 verbose info and wraps
	/// </summary>
	/// <param name="value"></param>
	void Verbose1(string? value);

	/// <summary>
	/// Logs level 2 verbose info and wraps
	/// </summary>
	/// <param name="value"></param>
	void Verbose2(string? value);

	/// <summary>
	/// Logs level 3 verbose info and wraps
	/// </summary>
	/// <param name="value"></param>
	void Verbose3(string? value);

	/// <summary>
	/// Logs exception and wraps
	/// </summary>
	/// <param name="value"></param>
	void Exception(Exception? value);

	/// <summary>
	/// Logs text with specified color and wraps
	/// </summary>
	/// <param name="value"></param>
	/// <param name="level"></param>
	/// <param name="color"></param>
	void Log(string? value, LogLevel level, ConsoleColor? color = null);

	/// <summary>
	/// Immediately flushes buffer and waits to clear buffer (for async mode)
	/// </summary>
	void Flush();

	/// <summary>
	/// Gets current logger with lock, current logger can be accessd only by the returned child logger
	/// </summary>
	/// <returns></returns>
	ILogger EnterLock();

	/// <summary>
	/// Exits lock mode and returns parent logger
	/// </summary>
	/// <returns></returns>
	ILogger ExitLock();
}
