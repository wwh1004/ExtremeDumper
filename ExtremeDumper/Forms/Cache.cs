using System;
using System.Drawing;

namespace ExtremeDumper.Forms {
	internal static class Cache {
		public static readonly bool Is64BitProcess = IntPtr.Size == 8;

		public static readonly Color DotNetColor = Color.YellowGreen;
	}
}
