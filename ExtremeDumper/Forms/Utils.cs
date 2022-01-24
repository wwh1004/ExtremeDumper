using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ExtremeDumper.Forms;

static unsafe class Utils {
	public static readonly bool Is64BitProcess = sizeof(nuint) == 8;

	public static readonly Color DotNetColor = Color.YellowGreen;

	public static void EnableDoubleBuffer(ListView listView) {
		typeof(ListView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, listView, new object[] { true });
	}

	public static string FormatHex(int value) {
		return $"0x{value:X8}";
	}

	public static string FormatHex(uint value) {
		return $"0x{value:X8}";
	}

	public static string FormatPointer(nint ptr) {
		return "0x" + ptr.ToString(Is64BitProcess ? "X16" : "X8");
	}

	public static string FormatPointer(void* ptr) {
		return "0x" + ((IntPtr)ptr).ToString(Is64BitProcess ? "X16" : "X8");
	}

	public static string FormatPointer(long ptr) {
		return "0x" + ptr.ToString(Is64BitProcess ? "X16" : "X8");
	}

	public static string FormatPointer(ulong ptr) {
		return "0x" + ptr.ToString(Is64BitProcess ? "X16" : "X8");
	}
}
