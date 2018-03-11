//using System;
//using System.Runtime.InteropServices;

//namespace InjectingDumper
//{
//    internal static class NativeMethods
//    {
//        public static readonly IntPtr CURRENT_PROCESS = (IntPtr)(-1);

//        public const uint MEM_COMMIT = 0x00001000;

//        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
//        public struct MEMORY_BASIC_INFORMATION
//        {
//            public IntPtr BaseAddress;

//            public IntPtr AllocationBase;

//            public uint AllocationProtect;

//            public IntPtr RegionSize;

//            public uint State;

//            public uint Protect;

//            public uint Type;

//            public static readonly uint Size = (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION));
//        }

//        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "ReadProcessMemory", ExactSpelling = true, SetLastError = true)]
//        [return: MarshalAs(UnmanagedType.Bool)]
//        public static unsafe extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, uint* lpNumberOfBytesRead);

//        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "VirtualQueryEx", ExactSpelling = true, SetLastError = true)]
//        [return: MarshalAs(UnmanagedType.Bool)]
//        public static extern bool VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

//        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "IsWow64Process", ExactSpelling = true, SetLastError = true)]
//        [return: MarshalAs(UnmanagedType.Bool)]
//        public static extern bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool Wow64Process);
//    }
//}
