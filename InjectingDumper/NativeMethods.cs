using System;
using System.Runtime.InteropServices;

namespace InjectingDumper
{
    internal static class NativeMethods
    {
        public static readonly IntPtr CURRENT_PROCESS = (IntPtr)(-1);

        public const uint PAGE_EXECUTE_READWRITE = 0x40;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;

            public IntPtr AllocationBase;

            public uint AllocationProtect;

            public IntPtr RegionSize;

            public uint State;

            public uint Protect;

            public uint Type;

            public static readonly uint Size = (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION));
        }

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "ReadProcessMemory", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out uint lpBuffer, uint nSize, uint* lpNumberOfBytesRead);

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "WriteProcessMemory", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref uint lpBuffer, uint nSize, uint* lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "VirtualProtect", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "VirtualQuery", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualQuery(IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);
    }
}
