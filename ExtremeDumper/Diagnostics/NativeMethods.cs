using System.Runtime.InteropServices;

namespace ExtremeDumper.Diagnostics;

static class NativeMethods {
	public const uint TH32CS_SNAPMODULE = 0x00000008;

	public const uint TH32CS_SNAPMODULE32 = 0x00000010;

	public static readonly nuint INVALID_HANDLE_VALUE = unchecked((nuint)(-1));

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct MODULEENTRY32 {
		public static readonly uint UnmanagedSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32));
		public static MODULEENTRY32 Default = new() { dwSize = UnmanagedSize };

		public uint dwSize;
		public uint th32ModuleID;
		public uint th32ProcessID;
		public uint GlblcntUsage;
		public uint ProccntUsage;
		public nuint modBaseAddr;
		public uint modBaseSize;
		public nuint hModule;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string szModule;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szExePath;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool CloseHandle(nuint hObject);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern nuint CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

	[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool Module32First(nuint hSnapshot, ref MODULEENTRY32 lpme);

	[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool Module32Next(nuint hSnapshot, ref MODULEENTRY32 lppe);
}
