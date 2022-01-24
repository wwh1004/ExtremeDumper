using System.Drawing;
using System.Runtime.InteropServices;

namespace ExtremeDumper.Forms;

static class NativeMethods {
	public const int OBJID_VSCROLL = unchecked((int)0xFFFFFFFB);

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct SCROLLBARINFO {
		public static readonly uint UnmanagedSize = (uint)Marshal.SizeOf(typeof(SCROLLBARINFO));
		public static SCROLLBARINFO Default = new() { cbSize = UnmanagedSize };

		public uint cbSize;
		public Rectangle rcScrollBar;
		public int dxyLineButton;
		public int xyThumbTop;
		public int xyThumbBottom;
		public int reserved;
		public fixed uint rgstate[6];
	}

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetScrollBarInfo(nuint hwnd, int idObject, ref SCROLLBARINFO psbi);
}
