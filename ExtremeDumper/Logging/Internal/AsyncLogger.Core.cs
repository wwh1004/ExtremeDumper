using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Tool.Logging;

partial class AsyncLogger {
	delegate void LogCallback(string value, LogLevel level, ConsoleColor? color);

	sealed class Context {
		public readonly object LockObj = new();

		public ILogger Creator;
		public volatile ILogger Owner;
		public volatile bool IsLocked;
		public readonly LogCallback Callback;

		public Context(ILogger creator, ILogger owner, LogCallback callback) {
			Creator = creator;
			Owner = owner;
			Callback = callback;
		}
	}

	sealed class LoggerCore {
		static readonly object logLock = new();
		static readonly ManualResetEvent asyncIdleEvent = new(true);
		static readonly Queue<LogItem> asyncQueue = new();
		static readonly object asyncLock = new();
		static readonly Thread asyncWorker = new(AsyncLoop) {
			Name = $"{nameof(AsyncLogger)}.{nameof(AsyncLoop)}",
			IsBackground = true
		};

		readonly Context context;
		LogLevel level;
		volatile bool isAsync;

		public Context Context => context;

		public LogLevel Level {
			get => level;
			set => level = value;
		}

		public bool IsAsync {
			get => isAsync;
			set {
				if (value == isAsync)
					return;

				lock (logLock) {
					isAsync = value;
					if (!value)
						Flush();
				}
			}
		}

		public static bool IsIdle => asyncQueue.Count == 0;

		public static int QueueCount => asyncQueue.Count;

		public LoggerCore(Context context) {
			this.context = context ?? throw new ArgumentNullException(nameof(context));
			level = LogLevel.Info;
			isAsync = true;
		}

		public void Info(ILogger logger) {
			Log(string.Empty, LogLevel.Info, null, logger);
		}

		public void Info(string? value, ILogger logger) {
			Log(value, LogLevel.Info, ConsoleColor.Gray, logger);
		}

		public void Warning(string? value, ILogger logger) {
			Log(value, LogLevel.Warning, ConsoleColor.Yellow, logger);
		}

		public void Error(string? value, ILogger logger) {
			Log(value, LogLevel.Error, ConsoleColor.Red, logger);
		}

		public void Verbose1(string? value, ILogger logger) {
			Log(value, LogLevel.Verbose1, ConsoleColor.DarkGray, logger);
		}

		public void Verbose2(string? value, ILogger logger) {
			Log(value, LogLevel.Verbose2, ConsoleColor.DarkGray, logger);
		}

		public void Verbose3(string? value, ILogger logger) {
			Log(value, LogLevel.Verbose3, ConsoleColor.DarkGray, logger);
		}

		public void Exception(Exception? value, ILogger logger) {
			Error(FormatException(value), logger);
		}

		public void Log(string? value, LogLevel level, ConsoleColor? color, ILogger logger) {
			if (context.IsLocked) {
			relock:
				if (context.Owner != logger) {
					lock (context.LockObj) {
						if (context.Owner != logger) {
							Monitor.Wait(context.LockObj);
							goto relock;
						}
					}
				}
			}

			if (level > Level)
				return;

			value ??= string.Empty;
			lock (logLock) {
				if (isAsync) {
					lock (asyncLock) {
						asyncQueue.Enqueue(new(context.Callback, value, level, color));
						if ((asyncWorker.ThreadState & ThreadState.Unstarted) != 0)
							asyncWorker.Start();
						Monitor.Pulse(asyncLock);
					}
				}
				else {
					context.Callback(value, level, color);
				}
			}
		}

		public static void Flush() {
		retry:
			asyncIdleEvent.WaitOne();
			if (!IsIdle) {
				Thread.Sleep(0);
				goto retry;
			}
			// AsyncLoop的执行线程可能停留在'Monitor.Wait(asyncLock)'这一行代码还没来得及相应
		}

		static void AsyncLoop() {
			var sb = new StringBuilder();
			while (true) {
				lock (asyncLock) {
					if (asyncQueue.Count == 0) {
						asyncIdleEvent.Set();
						Monitor.Wait(asyncLock);
					}
					asyncIdleEvent.Reset();
				}
				// 等待输出被触发

				LogItem[] logItems;
				lock (asyncLock) {
					logItems = asyncQueue.ToArray();
					asyncQueue.Clear();
				}
				var currentsByCallback = logItems.GroupBy(t => t.Callback).Select(t => new Queue<LogItem>(t)).ToArray();
				// 获取全部要输出的内容

				foreach (var currents in currentsByCallback) {
					// 按回调方法分组输出
					foreach (var logItem in currents) {
						if (string.IsNullOrEmpty(logItem.Value))
							logItem.Color = null;
						// 空行是什么颜色不重要，统一设置颜色为null
					}
					var callback = currents.Peek().Callback;
					do {
						var current = currents.Dequeue();
						var color = current.Color;
						sb.Length = 0;
						sb.Append(current.Value);
						while (true) {
							if (currents.Count == 0)
								break;

							var next = currents.Peek();
							if (next.Level != current.Level)
								break;

							if (!color.HasValue && next.Color.HasValue)
								color = next.Color;
							// 空行的颜色是null，获取第一个非null的颜色作为合并日志的颜色
							if (next.Color.HasValue && next.Color != color)
								break;
							// 如果下一行的颜色不是null并且与当前颜色不同，跳出优化

							sb.AppendLine();
							sb.Append(currents.Dequeue().Value);
						}
						// 合并日志等级与颜色相同的，减少重绘带来的性能损失
						callback(sb.ToString(), current.Level, color);
					} while (currents.Count > 0);
				}
			}
		}

		sealed class LogItem {
			public LogCallback Callback;
			public string Value;
			public LogLevel Level;
			public ConsoleColor? Color;

			public LogItem(LogCallback callback, string value, LogLevel level, ConsoleColor? color) {
				Callback = callback;
				Value = value;
				Level = level;
				Color = color;
			}
		}
	}
}
