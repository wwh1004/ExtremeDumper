using System;
using System.IO;
using System.Text;

namespace Tool.Logging;

/// <summary>
/// Default logger implement
/// </summary>
public sealed class DefaultLogger : AsyncLogger {
	static readonly byte[] Newline = Encoding.ASCII.GetBytes(Environment.NewLine);

	readonly bool writeConsole;
	readonly Stream? stream;
	readonly Encoding? encoding;

	/// <summary>
	/// Logger instance which only writes console
	/// </summary>
	public static ILogger ConsoleOnlyInstance { get; } = new DefaultLogger();

	/// <summary>
	/// Constructor
	/// Do NOT make it public! Multi <see cref="AsyncLogger.LogCallback"/> instances will cause <see cref="AsyncLogger.LoggerCore.AsyncLoop"/> slowly!
	/// </summary>
	DefaultLogger() {
		writeConsole = true;
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="writeConsole"></param>
	/// <param name="stream"></param>
	public DefaultLogger(bool writeConsole, Stream stream) : this(writeConsole, stream, Encoding.UTF8) {
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="writeConsole"></param>
	/// <param name="stream"></param>
	/// <param name="encoding"></param>
	public DefaultLogger(bool writeConsole, Stream stream, Encoding encoding) {
		this.writeConsole = writeConsole;
		this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
		this.encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
	}

	/// <inheritdoc />
	protected override void LogCore(string value, LogLevel level, ConsoleColor? color) {
		if (writeConsole) {
			ConsoleColor oldColor = default;
			if (color.HasValue) {
				oldColor = Console.ForegroundColor;
				Console.ForegroundColor = color.Value;
			}
			Console.WriteLine(value ?? string.Empty);
			if (color.HasValue)
				Console.ForegroundColor = oldColor;
		}
		if (stream is not null) {
			if (!string.IsNullOrEmpty(value)) {
				byte[] bytes = encoding!.GetBytes(value);
				stream.Write(bytes, 0, bytes.Length);
			}
			stream.Write(Newline, 0, Newline.Length);
			stream.Flush();
		}
	}
}
