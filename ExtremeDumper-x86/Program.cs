using System;
using System.IO;
using System.Reflection;

namespace ExtremeDumper_x86 {
	internal static class Program {
		[STAThread]
		private static void Main() {
			Assembly.LoadFile(Path.GetFullPath("ExtremeDumper.exe")).EntryPoint.Invoke(null, null);
		}
	}
}
