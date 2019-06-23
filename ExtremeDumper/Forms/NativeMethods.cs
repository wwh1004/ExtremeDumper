using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ExtremeDumper.Forms {
	internal static class NativeMethods {
		public const uint TH32CS_SNAPMODULE = 0x00000008;

		public const uint TH32CS_SNAPMODULE32 = 0x00000010;

		public static readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);

		public const int OBJID_VSCROLL = unchecked((int)0xFFFFFFFB);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct MODULEENTRY32 {
			public uint dwSize;

			public uint th32ModuleID;

			public uint th32ProcessID;

			public uint GlblcntUsage;

			public uint ProccntUsage;

			public IntPtr modBaseAddr;

			public uint modBaseSize;

			public IntPtr hModule;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string szModule;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szExePath;

			public static readonly uint UnmanagedSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32));

			public static MODULEENTRY32 Default { get => new MODULEENTRY32 { dwSize = UnmanagedSize }; }
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct SCROLLBARINFO {
			public uint cbSize;

			public Rectangle rcScrollBar;

			public int dxyLineButton;

			public int xyThumbTop;

			public int xyThumbBottom;

			public int reserved;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.U4)]
			public uint[] rgstate;

			public static readonly uint UnmanagedSize = (uint)Marshal.SizeOf(typeof(SCROLLBARINFO));

			public static SCROLLBARINFO Default { get => new SCROLLBARINFO { cbSize = UnmanagedSize }; }
		}

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "CreateToolhelp32Snapshot", ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "Module32FirstW", ExactSpelling = true, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

		[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "Module32NextW", ExactSpelling = true, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lppe);

		[DllImport("user32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "GetScrollBarInfo", ExactSpelling = true, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetScrollBarInfo(IntPtr hwnd, int idObject, ref SCROLLBARINFO psbi);
	}
}
