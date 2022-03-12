using System;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Tool.Logging;

/// <summary>
/// Async logger.
/// In derived class, you must override <see cref="LogCore"/>.
/// </summary>
public partial class AsyncLogger : ILogger {
	readonly LoggerCore core;
	bool isFreed;

	/// <inheritdoc />
	public virtual bool IsLocked => core.Context.IsLocked;

	/// <summary>
	/// Constructor
	/// </summary>
	protected AsyncLogger() {
		core = new LoggerCore(new Context(this, this, LogCore));
	}

	AsyncLogger(LoggerCore core) {
		this.core = core;
	}

	/// <summary>
	/// Immediately write the log without buffer. Derived class must override this method!
	/// </summary>
	/// <param name="value"></param>
	/// <param name="level"></param>
	/// <param name="color"></param>
	protected virtual void LogCore(string value, LogLevel level, ConsoleColor? color) {
		throw new NotImplementedException($"In derived class, you must override '{nameof(LogCore)}'");
	}

	/// <inheritdoc />
	public virtual ILogger EnterLock() {
		CheckFreed();
		var context = core.Context;
		if (this != context.Creator)
			throw new InvalidOperationException("Nested lock is not supported");

		relock:
		lock (context.LockObj) {
			if (context.IsLocked) {
				Monitor.Wait(context.LockObj);
				goto relock;
			}

			context.Owner = new AsyncLogger(core);
			context.IsLocked = true;
			return context.Owner;
		}
	}

	/// <inheritdoc />
	public virtual ILogger ExitLock() {
		CheckFreed();
		var context = core.Context;
		if (context.Creator == this)
			throw new InvalidOperationException("No lock can be exited");

		isFreed = true;
		context.Owner = context.Creator;
		context.IsLocked = false;
		lock (context.LockObj)
			Monitor.PulseAll(context.LockObj);
		return context.Owner;
	}

	/// <summary>
	/// Checks current logger is freed
	/// </summary>
	protected void CheckFreed() {
		if (isFreed)
			throw new InvalidOperationException("Current logger is freed");
	}

	/// <summary>
	/// Format exception
	/// </summary>
	/// <param name="exception"></param>
	/// <returns></returns>
	protected static string FormatException(Exception? exception) {
		var sb = new StringBuilder();
		DumpException(exception, sb);
		return sb.ToString();
	}

	static void DumpException(Exception? exception, StringBuilder sb) {
		exception ??= new ArgumentNullException(nameof(exception), "<No exception object>");
		sb.AppendLine($"Type: {Environment.NewLine}{exception.GetType().FullName}");
		sb.AppendLine($"Message: {Environment.NewLine}{exception.Message}");
		sb.AppendLine($"Source: {Environment.NewLine}{exception.Source}");
		sb.AppendLine($"StackTrace: {Environment.NewLine}{exception.StackTrace}");
		sb.AppendLine($"TargetSite: {Environment.NewLine}{exception.TargetSite}");
		sb.AppendLine("----------------------------------------");
		if (exception.InnerException is not null)
			DumpException(exception.InnerException, sb);
		if (exception is ReflectionTypeLoadException reflectionTypeLoadException) {
			foreach (var loaderException in reflectionTypeLoadException.LoaderExceptions)
				DumpException(loaderException, sb);
		}
	}

	#region forwards
	/// <inheritdoc />
	public virtual LogLevel Level {
		get {
			CheckFreed();
			return core.Level;
		}
		set {
			CheckFreed();
			core.Level = value;
		}
	}

	/// <inheritdoc />
	public virtual bool IsAsync {
		get {
			CheckFreed();
			return core.IsAsync;
		}
		set {
			CheckFreed();
			core.IsAsync = value;
		}
	}

	/// <inheritdoc />
	public virtual bool IsIdle {
		get {
			CheckFreed();
			return LoggerCore.IsIdle;
		}
	}

	/// <inheritdoc />
	public virtual int QueueCount {
		get {
			CheckFreed();
			return LoggerCore.QueueCount;
		}
	}

	/// <inheritdoc />
	public virtual void Info() {
		CheckFreed();
		core.Info(this);
	}

	/// <inheritdoc />
	public virtual void Info(string? value) {
		CheckFreed();
		core.Info(value, this);
	}

	/// <inheritdoc />
	public virtual void Warning(string? value) {
		CheckFreed();
		core.Warning(value, this);
	}

	/// <inheritdoc />
	public virtual void Error(string? value) {
		CheckFreed();
		core.Error(value, this);
	}

	/// <inheritdoc />
	public virtual void Verbose1(string? value) {
		CheckFreed();
		core.Verbose1(value, this);
	}

	/// <inheritdoc />
	public virtual void Verbose2(string? value) {
		CheckFreed();
		core.Verbose2(value, this);
	}

	/// <inheritdoc />
	public virtual void Verbose3(string? value) {
		CheckFreed();
		core.Verbose3(value, this);
	}

	/// <inheritdoc />
	public virtual void Exception(Exception? value) {
		CheckFreed();
		core.Exception(value, this);
	}

	/// <inheritdoc />
	public virtual void Log(string? value, LogLevel level, ConsoleColor? color = null) {
		CheckFreed();
		core.Log(value, level, color, this);
	}

	/// <inheritdoc />
	public virtual void Flush() {
		CheckFreed();
		LoggerCore.Flush();
	}
	#endregion
}
