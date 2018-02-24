using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace ExtremeDumper.MegaDumper
{
    public class MegaDumper64 : IDumper
    {
        private uint _processId;

        public MegaDumper64(uint processId)
        {
            _processId = processId;
        }

        public bool DumpModule(IntPtr moduleHandle, string path)
        {
            throw new NotSupportedException();
        }

        public bool DumpModule(uint moduleId, string path)
        {
            throw new NotSupportedException();
        }

        public int DumpProcess(string path)
        {
            return MegaDumperPrivate.DumpProcess(_processId, path);
        }

        private static class MegaDumperPrivate
        {
            [DllImport("kernel32.dll")]
            private static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

            [DllImport("kernel32.dll")]
            private static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

            [DllImport("Kernel32.dll")]
            private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, ref uint lpNumberOfBytesRead);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr hObject);

            private static ulong minAddress;

            private static ulong maxAddress;

            private static uint PageSize;

            static MegaDumperPrivate()
            {
                minAddress = 0;
                maxAddress = long.MaxValue;
                PageSize = 0x1000;
                SYSTEM_INFO system_INFO = default(SYSTEM_INFO);
                GetNativeSystemInfo(ref system_INFO);
                minAddress = (ulong)((long)system_INFO.lpMinimumApplicationAddress);
                maxAddress = (ulong)((long)system_INFO.lpMaximumApplicationAddress);
                PageSize = system_INFO.dwPageSize;
            }

            public static unsafe int DumpProcess(uint processId, string DirectoryName)
            {
                IntPtr processHandle;

                processHandle = OpenProcess(1080u, 0, processId);
                if (processHandle == IntPtr.Zero)
                    return 0;
                try
                {
                    MegaDumpDirectoryHelper.CreateDirectories(DirectoryName);
                    int num2 = 1;
                    MEMORY_BASIC_INFORMATION memory_BASIC_INFORMATION;
                    for (ulong num3 = minAddress; num3 < maxAddress; num3 = memory_BASIC_INFORMATION.BaseAddress + memory_BASIC_INFORMATION.RegionSize)
                    {
                        VirtualQueryEx(processHandle, (IntPtr)num3, out memory_BASIC_INFORMATION, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
                        if (memory_BASIC_INFORMATION.State == 4096)
                        {
                            byte[] array = new byte[memory_BASIC_INFORMATION.RegionSize];
                            uint num4 = 0u;
                            byte[] array2 = new byte[8];
                            bool flag = ReadProcessMemory(processHandle, (IntPtr)((long)num3), array, (uint)array.Length, ref num4);
                            if (flag)
                            {
                                for (int i = 0; i < array.Length - 2; i++)
                                {
                                    if (array[i] == 77 && array[i + 1] == 90)
                                    {
                                        if (ReadProcessMemory(processHandle, (IntPtr)((long)(num3 + (ulong)i + 60UL)), array2, 4u, ref num4))
                                        {
                                            int num5 = BitConverter.ToInt32(array2, 0);
                                            if (num5 > 0 && num5 + 288 < array.Length)
                                            {
                                                if (ReadProcessMemory(processHandle, (IntPtr)((long)(num3 + (ulong)i + (ulong)num5)), array2, 2u, ref num4))
                                                {
                                                    if (array2[0] == 80 && array2[1] == 69)
                                                    {
                                                        long num6 = 0L;
                                                        try
                                                        {
                                                            if (ReadProcessMemory(processHandle, (IntPtr)((long)(num3 + (ulong)i + (ulong)num5 + 248UL)), array2, 8u, ref num4))
                                                                num6 = BitConverter.ToInt64(array2, 0);
                                                        }
                                                        catch
                                                        {
                                                        }
                                                        #region Dump Native
                                                        byte[] array3 = new byte[PageSize];
                                                        if (ReadProcessMemory(processHandle, (IntPtr)((long)(num3 + (ulong)i)), array3, (uint)array3.Length, ref num4))
                                                        {
                                                            int num7 = BitConverter.ToInt16(array3, num5 + 6);
                                                            if (num7 > 0)
                                                            {
                                                                int num8 = BitConverter.ToInt32(array3, num5 + 56);
                                                                int num9 = BitConverter.ToInt32(array3, num5 + 60);
                                                                short num10 = BitConverter.ToInt16(array3, num5 + 20);
                                                                bool isDll = false;
                                                                if ((array3[num5 + 23] & 32) != 0)
                                                                {
                                                                    isDll = true;
                                                                }
                                                                IntPtr ptr = IntPtr.Zero;
                                                                IMAGE_SECTION_HEADER[] array4 = new IMAGE_SECTION_HEADER[num7];
                                                                ulong num11 = num3 + (ulong)i + (ulong)num5 + (ulong)num10 + 4UL + (ulong)Marshal.SizeOf(typeof(IMAGE_FILE_HEADER));
                                                                for (int j = 0; j < num7; j++)
                                                                {
                                                                    byte[] array5 = new byte[Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER))];
                                                                    ReadProcessMemory(processHandle, (IntPtr)((long)num11), array5, (uint)array5.Length, ref num4);
                                                                    fixed (byte* ptr2 = array5)
                                                                    {
                                                                        ptr = (IntPtr)((void*)ptr2);
                                                                    }
                                                                    array4[j] = (IMAGE_SECTION_HEADER)Marshal.PtrToStructure(ptr, typeof(IMAGE_SECTION_HEADER));
                                                                    num11 += (ulong)Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER));
                                                                }
                                                                int num12 = 0;
                                                                int size_of_raw_data = array4[num7 - 1].size_of_raw_data;
                                                                int pointer_to_raw_data = array4[num7 - 1].pointer_to_raw_data;
                                                                if (size_of_raw_data > 0 && pointer_to_raw_data > 0)
                                                                {
                                                                    num12 = size_of_raw_data + pointer_to_raw_data;
                                                                }
                                                                int num13 = BitConverter.ToInt32(array3, num5 + 80);
                                                                int num14 = num13;
                                                                int num15 = array4[0].pointer_to_raw_data;
                                                                int num16 = 0;
                                                                for (int j = 0; j < num7; j++)
                                                                {
                                                                    int virtual_size = array4[j].virtual_size;
                                                                    int num17 = virtual_size % num8;
                                                                    if (num17 != 0)
                                                                    {
                                                                        num17 = num8 - num17;
                                                                    }
                                                                    num15 = num15 + virtual_size + num17;
                                                                }
                                                                if (num15 > num14)
                                                                {
                                                                    num14 = num15;
                                                                }
                                                                try
                                                                {
                                                                    byte[] array6 = new byte[num12];
                                                                }
                                                                catch
                                                                {
                                                                    num12 = num14;
                                                                }
                                                                if (num12 != 0)
                                                                {
                                                                    byte[] array7 = new byte[num12];
                                                                    try
                                                                    {
                                                                        if (ReadProcessMemory(processHandle, (IntPtr)((long)(num3 + (ulong)i)), array7, (uint)array7.Length, ref num4))
                                                                        {
                                                                            string filePath1 = Path.Combine(DirectoryName, "rawdump_" + (num3 + (ulong)i).ToString("X16"));
                                                                            if (File.Exists(filePath1))
                                                                                filePath1 = Path.Combine(DirectoryName, "rawdump" + num2.ToString() + "_" + (num3 + (ulong)i).ToString("X16"));
                                                                            if (isDll)
                                                                                filePath1 += ".dll";
                                                                            else
                                                                                filePath1 += ".exe";
                                                                            File.WriteAllBytes(filePath1, array7);
                                                                            num2++;
                                                                        }
                                                                    }
                                                                    catch
                                                                    {
                                                                    }
                                                                }
                                                                byte[] array8 = new byte[num14];
                                                                Array.Copy(array3, array8, (long)((ulong)PageSize));
                                                                int num18 = 0;
                                                                for (int k = 0; k < num7; k++)
                                                                {
                                                                    int num19 = array4[k].size_of_raw_data;
                                                                    int num20 = array4[k].pointer_to_raw_data;
                                                                    int virtual_size = array4[k].virtual_size;
                                                                    num16 = array4[k].virtual_address;
                                                                    #region Dump RAW
                                                                    int num21 = virtual_size % num9;
                                                                    if (num21 != 0)
                                                                    {
                                                                        num21 = num9 - num21;
                                                                    }
                                                                    num21 = virtual_size + num21;
                                                                    if ((num21 != 0 && num19 != num21 && num19 != virtual_size) || num20 < 0)
                                                                    {
                                                                        num19 = virtual_size;
                                                                        num20 = num16;
                                                                        BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(array8));
                                                                        binaryWriter.BaseStream.Position = num5 + 232 + 40 * k + 20 + 28;
                                                                        binaryWriter.Write(virtual_size);
                                                                        binaryWriter.BaseStream.Position = num5 + 232 + 40 * k + 24 + 28;
                                                                        binaryWriter.Write(num16);
                                                                        binaryWriter.Close();
                                                                    }
                                                                    #endregion
                                                                    byte[] array9 = new byte[0];
                                                                    try
                                                                    {
                                                                        array9 = new byte[num19];
                                                                    }
                                                                    catch
                                                                    {
                                                                        array9 = new byte[virtual_size];
                                                                    }
                                                                    int num22 = array9.Length;
                                                                    flag = ReadProcessMemory(processHandle, (IntPtr)((long)(num3 + (ulong)i + (ulong)num16)), array9, (uint)num19, ref num4);
                                                                    if (!flag || num4 != (ulong)num19)
                                                                    {
                                                                        num22 = 0;
                                                                        byte[] array10 = new byte[PageSize];
                                                                        for (int l = 0; l < num19; l += (int)PageSize)
                                                                        {
                                                                            try
                                                                            {
                                                                                flag = ReadProcessMemory(processHandle, (IntPtr)((long)(num3 + (ulong)i + (ulong)num16 + (ulong)l)), array10, PageSize, ref num4);
                                                                            }
                                                                            catch
                                                                            {
                                                                                break;
                                                                            }
                                                                            if (flag)
                                                                            {
                                                                                num22 += (int)PageSize;
                                                                                int j = 0;
                                                                                while (j < (long)((ulong)PageSize))
                                                                                {
                                                                                    if (l + j < array9.Length)
                                                                                        array9[l + j] = array10[j];
                                                                                    j++;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    try
                                                                    {
                                                                        Array.Copy(array9, 0, array8, num20, num22);
                                                                    }
                                                                    catch
                                                                    {
                                                                    }
                                                                    if (k == num7 - 1)
                                                                        num18 = num20 + num19;
                                                                }
                                                                string filePath2 = Path.Combine(DirectoryName, "vdump_" + (num3 + (ulong)i).ToString("X16"));
                                                                if (File.Exists(filePath2))
                                                                    filePath2 = Path.Combine(DirectoryName, "vdump" + num2.ToString() + "_" + (num3 + (ulong)i).ToString("X16"));
                                                                if (isDll)
                                                                    filePath2 += ".dll";
                                                                else
                                                                    filePath2 += ".exe";
                                                                using (FileStream fileStream = new FileStream(filePath2, FileMode.Create))
                                                                    fileStream.Write(array8, 0, Math.Min(num18, array8.Length));
                                                                num2++;
                                                            }
                                                        }
                                                        #endregion
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #region 修复文件名
                    foreach (FileInfo fileInfo in new DirectoryInfo(DirectoryName).GetFiles())
                    {
                        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(fileInfo.FullName);
                        if (versionInfo.OriginalFilename != null && versionInfo.OriginalFilename != "")
                        {
                            string filePath3 = Path.Combine(DirectoryName, versionInfo.OriginalFilename);
                            int repetition = 2;
                            if (File.Exists(filePath3))
                            {
                                string extension = Path.GetExtension(filePath3);
                                if (extension == "")
                                    extension = ".dll";
                                do
                                {
                                    filePath3 = Path.Combine(DirectoryName, Path.GetFileNameWithoutExtension(versionInfo.OriginalFilename) + "(" + repetition.ToString() + ")" + extension);
                                    repetition++;
                                }
                                while (File.Exists(filePath3));
                            }
                            File.Move(fileInfo.FullName, filePath3);
                        }
                    }
                    MegaDumpDirectoryHelper.Classify(DirectoryName);
                    #endregion
                    num2--;
                    return num2;
                }
                finally
                {
                    CloseHandle(processHandle);
                }
            }

            private struct SYSTEM_INFO
            {
                public ushort wProcessorArchitecture;

                public ushort wReserved;

                public uint dwPageSize;

                public IntPtr lpMinimumApplicationAddress;

                public IntPtr lpMaximumApplicationAddress;

                public UIntPtr dwActiveProcessorMask;

                public uint dwNumberOfProcessors;

                public uint dwProcessorType;

                public uint dwAllocationGranularity;

                public ushort wProcessorLevel;

                public ushort wProcessorRevision;
            }

            private struct MEMORY_BASIC_INFORMATION
            {
                public ulong BaseAddress;

                public ulong AllocationBase;

                public int AllocationProtect;

                public ulong RegionSize;

                public int State;

                public ulong Protect;

                public ulong Type;
            }

#pragma warning disable 0649
            private unsafe struct IMAGE_SECTION_HEADER
            {
                public fixed byte name[8];

                public int virtual_size;

                public int virtual_address;

                public int size_of_raw_data;

                public int pointer_to_raw_data;

                public int pointer_to_relocations;

                public int pointer_to_linenumbers;

                public short number_of_relocations;

                public short number_of_linenumbers;

                public int characteristics;
            }

            private struct IMAGE_FILE_HEADER
            {
                public short Machine;

                public short NumberOfSections;

                public int TimeDateStamp;

                public int PointerToSymbolTable;

                public int NumberOfSymbols;

                public short SizeOfOptionalHeader;

                public short Characteristics;
            }
#pragma warning restore 0649
        }
    }
}
