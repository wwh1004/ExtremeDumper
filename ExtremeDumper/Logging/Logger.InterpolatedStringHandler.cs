using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Tool.Logging;

namespace ExtremeDumper.Logging;

partial class Logger {
	[EditorBrowsable(EditorBrowsableState.Never)]
	[InterpolatedStringHandler]
	public struct Verbose1InterpolatedStringHandler {
		private AppendInterpolatedStringHandler _stringBuilderHandler;

		public Verbose1InterpolatedStringHandler(int literalLength, int formattedCount, out bool shouldAppend) {
			if (Level < LogLevel.Verbose1) {
				_stringBuilderHandler = default;
				shouldAppend = false;
			}
			else {
				_stringBuilderHandler = new AppendInterpolatedStringHandler(literalLength, formattedCount, new StringBuilder());
				shouldAppend = true;
			}
		}

		internal string? ToStringAndClear() {
			string? s = _stringBuilderHandler._stringBuilder?.ToString();
			_stringBuilderHandler = default;
			return s;
		}

		public void AppendLiteral(string value) {
			_stringBuilderHandler.AppendLiteral(value);
		}

		public void AppendFormatted<T>(T value) {
			_stringBuilderHandler.AppendFormatted(value);
		}

		public void AppendFormatted<T>(T value, string? format) {
			_stringBuilderHandler.AppendFormatted(value, format);
		}

		public void AppendFormatted<T>(T value, int alignment) {
			_stringBuilderHandler.AppendFormatted(value, alignment);
		}

		public void AppendFormatted<T>(T value, int alignment, string? format) {
			_stringBuilderHandler.AppendFormatted(value, alignment, format);
		}

		public void AppendFormatted(string? value) {
			_stringBuilderHandler.AppendFormatted(value);
		}

		public void AppendFormatted(string? value, int alignment = 0, string? format = null) {
			_stringBuilderHandler.AppendFormatted(value, alignment, format);
		}

		public void AppendFormatted(object? value, int alignment = 0, string? format = null) {
			_stringBuilderHandler.AppendFormatted(value, alignment, format);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[InterpolatedStringHandler]
	public struct Verbose2InterpolatedStringHandler {
		private AppendInterpolatedStringHandler _stringBuilderHandler;

		public Verbose2InterpolatedStringHandler(int literalLength, int formattedCount, out bool shouldAppend) {
			if (Level < LogLevel.Verbose2) {
				_stringBuilderHandler = default;
				shouldAppend = false;
			}
			else {
				_stringBuilderHandler = new AppendInterpolatedStringHandler(literalLength, formattedCount, new StringBuilder());
				shouldAppend = true;
			}
		}

		internal string? ToStringAndClear() {
			string? s = _stringBuilderHandler._stringBuilder?.ToString();
			_stringBuilderHandler = default;
			return s;
		}

		public void AppendLiteral(string value) {
			_stringBuilderHandler.AppendLiteral(value);
		}

		public void AppendFormatted<T>(T value) {
			_stringBuilderHandler.AppendFormatted(value);
		}

		public void AppendFormatted<T>(T value, string? format) {
			_stringBuilderHandler.AppendFormatted(value, format);
		}

		public void AppendFormatted<T>(T value, int alignment) {
			_stringBuilderHandler.AppendFormatted(value, alignment);
		}

		public void AppendFormatted<T>(T value, int alignment, string? format) {
			_stringBuilderHandler.AppendFormatted(value, alignment, format);
		}

		public void AppendFormatted(string? value) {
			_stringBuilderHandler.AppendFormatted(value);
		}

		public void AppendFormatted(string? value, int alignment = 0, string? format = null) {
			_stringBuilderHandler.AppendFormatted(value, alignment, format);
		}

		public void AppendFormatted(object? value, int alignment = 0, string? format = null) {
			_stringBuilderHandler.AppendFormatted(value, alignment, format);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[InterpolatedStringHandler]
	public struct Verbose3InterpolatedStringHandler {
		private AppendInterpolatedStringHandler _stringBuilderHandler;

		public Verbose3InterpolatedStringHandler(int literalLength, int formattedCount, out bool shouldAppend) {
			if (Level < LogLevel.Verbose3) {
				_stringBuilderHandler = default;
				shouldAppend = false;
			}
			else {
				_stringBuilderHandler = new AppendInterpolatedStringHandler(literalLength, formattedCount, new StringBuilder());
				shouldAppend = true;
			}
		}

		internal string? ToStringAndClear() {
			string? s = _stringBuilderHandler._stringBuilder?.ToString();
			_stringBuilderHandler = default;
			return s;
		}

		public void AppendLiteral(string value) {
			_stringBuilderHandler.AppendLiteral(value);
		}

		public void AppendFormatted<T>(T value) {
			_stringBuilderHandler.AppendFormatted(value);
		}

		public void AppendFormatted<T>(T value, string? format) {
			_stringBuilderHandler.AppendFormatted(value, format);
		}

		public void AppendFormatted<T>(T value, int alignment) {
			_stringBuilderHandler.AppendFormatted(value, alignment);
		}

		public void AppendFormatted<T>(T value, int alignment, string? format) {
			_stringBuilderHandler.AppendFormatted(value, alignment, format);
		}

		public void AppendFormatted(string? value) {
			_stringBuilderHandler.AppendFormatted(value);
		}

		public void AppendFormatted(string? value, int alignment = 0, string? format = null) {
			_stringBuilderHandler.AppendFormatted(value, alignment, format);
		}

		public void AppendFormatted(object? value, int alignment = 0, string? format = null) {
			_stringBuilderHandler.AppendFormatted(value, alignment, format);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[InterpolatedStringHandler]
	struct AppendInterpolatedStringHandler {
		internal readonly StringBuilder _stringBuilder;
		private readonly IFormatProvider? _provider;

		public AppendInterpolatedStringHandler(int literalLength, int formattedCount, StringBuilder stringBuilder) {
			_stringBuilder = stringBuilder;
			_provider = null;
		}

		public AppendInterpolatedStringHandler(int literalLength, int formattedCount, StringBuilder stringBuilder, IFormatProvider? provider) {
			_stringBuilder = stringBuilder;
			_provider = provider;
		}

		public void AppendLiteral(string value) {
			_stringBuilder.Append(value);
		}

		public void AppendFormatted<T>(T value) {
			if (value is IFormattable) {
				_stringBuilder.Append(((IFormattable)value).ToString(format: null, _provider));
			}
			else if (value is not null) {
				_stringBuilder.Append(value.ToString());
			}
		}

		public void AppendFormatted<T>(T value, string? format) {
			if (value is IFormattable) {
				_stringBuilder.Append(((IFormattable)value).ToString(format, _provider));
			}
			else if (value is not null) {
				_stringBuilder.Append(value.ToString());
			}
		}

		public void AppendFormatted<T>(T value, int alignment) {
			AppendFormatted(value, alignment, format: null);
		}

		public void AppendFormatted<T>(T value, int alignment, string? format) {
			if (alignment == 0) {
				AppendFormatted(value, format);
			}
			else if (alignment < 0) {
				int start = _stringBuilder.Length;
				AppendFormatted(value, format);
				int paddingRequired = -alignment - (_stringBuilder.Length - start);
				if (paddingRequired > 0) {
					_stringBuilder.Append(' ', paddingRequired);
				}
			}
			else {
				string s;
				if (value is IFormattable) {
					s = ((IFormattable)value).ToString(format, _provider);
				}
				else {
					s = value?.ToString() ?? string.Empty;
				}
				AppendFormatted(s, alignment);
			}
		}

		public void AppendFormatted(string? value) {
			_stringBuilder.Append(value);
		}

		public void AppendFormatted(string? value, int alignment = 0, string? format = null) {
			AppendFormatted<string?>(value, alignment, format);
		}

		public void AppendFormatted(object? value, int alignment = 0, string? format = null) {
			AppendFormatted<object?>(value, alignment, format);
		}
	}
}
