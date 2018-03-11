//using System;
//using System.Runtime.InteropServices;
//using static InjectingDumper.NativeMethods;

//namespace InjectingDumper
//{
//    internal delegate bool EnumPagesCallback(MEMORY_BASIC_INFORMATION mbi);

//    internal static class MemoryIO
//    {
//        internal static unsafe bool ReadBytes(IntPtr addr, byte[] value) => ReadProcessMemory(CURRENT_PROCESS, addr, value, (uint)value.Length, null);

//        internal static unsafe bool EnumPages(IntPtr startAddress, EnumPagesCallback callback)
//        {
//            bool is64;
//            IntPtr nextAddress;
//            MEMORY_BASIC_INFORMATION mbi;

//            if (!Is64BitProcess(CURRENT_PROCESS, out is64))
//                return false;
//            if (is64)
//            {
//                nextAddress = startAddress;
//                do
//                {
//                    if (!VirtualQueryEx(CURRENT_PROCESS, nextAddress, out mbi, MEMORY_BASIC_INFORMATION.Size))
//                        return Marshal.GetLastWin32Error() == 87;
//                    if ((mbi.State & MEM_COMMIT) == MEM_COMMIT && mbi.Protect != 0)
//                        if (!callback(mbi))
//                            return true;
//                    nextAddress = (IntPtr)((long)mbi.BaseAddress + (long)mbi.RegionSize);
//                } while ((long)nextAddress > 0);
//            }
//            else
//            {
//                nextAddress = startAddress;
//                if ((ulong)nextAddress > int.MaxValue)
//                    return false;
//                do
//                {
//                    if (!VirtualQueryEx(CURRENT_PROCESS, nextAddress, out mbi, MEMORY_BASIC_INFORMATION.Size))
//                        return false;
//                    if ((mbi.State & MEM_COMMIT) == MEM_COMMIT && mbi.Protect != 0)
//                        if (!callback(mbi))
//                            return true;
//                    nextAddress = (IntPtr)((int)mbi.BaseAddress + (int)mbi.RegionSize);
//                } while ((int)nextAddress > 0);
//            }
//            return true;
//        }

//        private static bool Is64BitProcess(IntPtr processHandle, out bool is64)
//        {
//            bool isWow64;

//            if (!Environment.Is64BitOperatingSystem)
//            {
//                //不是64位系统肯定不会是64位进程
//                is64 = false;
//                return true;
//            }
//            if (!IsWow64Process(processHandle, out isWow64))
//            {
//                //执行失败
//                is64 = false;
//                return false;
//            }
//            is64 = !isWow64;
//            return true;
//        }
//    }
//}
