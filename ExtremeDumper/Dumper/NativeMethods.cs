using System;
using System.Runtime.InteropServices;
using size_t = System.IntPtr;

namespace ExtremeDumper.Dumper
{
    internal static unsafe class NativeMethods
    {
        public const uint PROCESS_QUERY_INFORMATION = 0x0400;

        public const uint PROCESS_VM_READ = 0x0010;

        public static readonly IntPtr CURRENT_PROCESS = (IntPtr)(-1);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SYSTEM_INFO
        {
            public uint dwOemId;

            public uint dwPageSize;

            public uint lpMinimumApplicationAddress;

            public uint lpMaximumApplicationAddress;

            public uint dwActiveProcessorMask;

            public uint dwNumberOfProcessors;

            public uint dwProcessorType;

            public uint dwAllocationGranularity;

            public uint dwProcessorLevel;

            public uint dwProcessorRevision;
        }

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "GetSystemInfo", ExactSpelling = true, SetLastError = true)]
        public static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "OpenProcess", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "CloseHandle", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "IsWow64Process", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool Wow64Process);

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "ReadProcessMemory", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, size_t nSize, size_t* lpNumberOfBytesRead);

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "ReadProcessMemory", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte* lpBuffer, size_t nSize, size_t* lpNumberOfBytesRead);
    }
}
