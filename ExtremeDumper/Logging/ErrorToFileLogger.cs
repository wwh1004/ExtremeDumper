using System;
using System.IO;
using System.Text;
using Tool.Logging;

namespace ExtremeDumper.Logging;

sealed class ErrorToFileLogger : ILogger {
	readonly ILogger logger;
	readonly FileStream logFile;

	public LogLevel Level {
		get => logger.Level;
		set => logger.Level = value;
	}

	public bool IsAsync {
		get => logger.IsAsync;
		set => logger.IsAsync = value;
	}

	public bool IsIdle => logger.IsIdle;

	public int QueueCount => logger.QueueCount;

	public bool IsLocked => logger.IsLocked;

	public ErrorToFileLogger(ILogger logger, FileStream logFile) {
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.logFile = logFile ?? throw new ArgumentNullException(nameof(logFile));
	}

	public void Info() {
		logger.Info();
	}

	public void Info(string? value) {
		logger.Info(value);
	}

	public void Warning(string? value) {
		WriteFile(value);
		logger.Warning(value);
	}

	public void Error(string? value) {
		WriteFile(value);
		logger.Error(value);
	}

	public void Verbose1(string? value) {
		logger.Verbose1(value);
	}

	public void Verbose2(string? value) {
		logger.Verbose2(value);
	}

	public void Verbose3(string? value) {
		logger.Verbose3(value);
	}

	public void Exception(Exception? value) {
		logger.Exception(value);
	}

	public void Log(string? value, LogLevel level, ConsoleColor? color) {
		logger.Log(value, level, color);
	}

	public void Flush() {
		logger.Flush();
		logFile.Flush();
	}

	public ILogger EnterLock() {
		return logger.EnterLock();
	}

	public ILogger ExitLock() {
		return logger.ExitLock();
	}

	void WriteFile(string? value) {
		value += Environment.NewLine;
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		logFile.Write(bytes, 0, bytes.Length);
	}
}
