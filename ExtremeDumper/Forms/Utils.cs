using System;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace ExtremeDumper.Forms;

static unsafe class Utils {
	static readonly char[] obfuscationChars1 = { '`', '~', '^', '*', '\'', '"', '+', '.' };
	static readonly char[] obfuscationChars2 = "ÅBĊĎĘḞĢΉÎĴĶḼḾŅÕҎQŖŜTỰṼẂẌẎẐ".ToCharArray();
	static readonly char[] obfuscationChars3 = "åbċďęḟģήîĵķḽḿņõҏqŗŝtựṽẃẍẏẑ".ToCharArray();

	public static readonly bool Is64BitProcess = sizeof(nuint) == 8;

	public static readonly Color DotNetColor = Color.YellowGreen;

	public static void EnableDoubleBuffer(ListView listView) {
		typeof(ListView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, listView, new object[] { true });
	}

	public static string ObfuscateTitle(string title) {
		var random = new Random();
		var sb = new StringBuilder(title.Length * 2);
		for (int i = 0; i < title.Length - 1; i++) {
			char c = title[i];
			if ('A' <= c && c <= 'Z')
				sb.Append(obfuscationChars2[c - 'A']);
			else if ('a' <= c && c <= 'z')
				sb.Append(obfuscationChars3[c - 'a']);
			else
				sb.Append(c);
		}
		sb.Append(title[title.Length - 1]);
		return sb.ToString();
	}

	public static string ObfuscateTitleAlternative(string title) {
		var random = new Random();
		var sb = new StringBuilder(title.Length * 2);
		for (int i = 0; i < title.Length - 1; i++) {
			sb.Append(title[i]);
			sb.Append(obfuscationChars1[random.Next(obfuscationChars1.Length)]);
		}
		sb.Append(title[title.Length - 1]);
		return sb.ToString();
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
