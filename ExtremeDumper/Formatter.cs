using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtremeDumper;

static unsafe class Formatter {
	public static string Format(object? o, int nameMaxLength = 512) {
		if (o is byte[] b)
			return BitConverter.ToString(b).Replace("-", string.Empty);

		if (o is string s)
			return s;

		if (o is null)
			return "<null>";

		if (o is IEnumerable<object> a)
			return string.Join(",", a.Select(t => Format(t, nameMaxLength)));

		return o.ToString();
	}

	public static string FormatHex(sbyte value) {
		return $"0x{value:X2}";
	}

	public static string FormatHex(byte value) {
		return $"0x{value:X2}";
	}

	public static string FormatHex(short value) {
		return $"0x{value:X4}";
	}

	public static string FormatHex(ushort value) {
		return $"0x{value:X4}";
	}

	public static string FormatHex(int value) {
		return $"0x{value:X8}";
	}

	public static string FormatHex(uint value) {
		return $"0x{value:X8}";
	}

	public static string FormatHex(long value) {
		return $"0x{value:X16}";
	}

	public static string FormatHex(ulong value) {
		return $"0x{value:X16}";
	}

	public static string FormatHex(nint value) {
		return sizeof(nint) == 4 ? $"0x{(int)value:X8}" : $"0x{(long)value:X16}";
	}

	public static string FormatHex(nuint value) {
		return sizeof(nuint) == 4 ? $"0x{(uint)value:X8}" : $"0x{(ulong)value:X16}";
	}

	public static string FormatHexAuto(int value) {
		return $"0x{value:X}";
	}

	public static string FormatHexAuto(uint value) {
		return $"0x{value:X}";
	}

	public static string FormatHexAuto(long value) {
		return $"0x{value:X}";
	}

	public static string FormatHexAuto(ulong value) {
		return $"0x{value:X}";
	}

	public static string FormatHexAuto(nint value) {
		return $"0x{(ulong)value:X}";
	}

	public static string FormatHexAuto(nuint value) {
		return $"0x{(ulong)value:X}";
	}
}
