using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace ExtremeDumper.MegaDumper
{
    public class MegaDumper32 : IDumper
    {
        private uint _processId;

        public MegaDumper32(uint processId)
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
            [DllImport("Kernel32.dll")]
            private static extern bool ReadProcessMemory
            (
                IntPtr hProcess,
                IntPtr lpBaseAddress,
                byte[] lpBuffer,
                UInt32 nSize,
                ref UInt32 lpNumberOfBytesRead
            );

            [DllImport("Kernel32.dll")]
            private static extern bool ReadProcessMemory
            (
                IntPtr hProcess,
                uint lpBaseAddress,
                byte[] lpBuffer,
                UInt32 nSize,
                ref UInt32 lpNumberOfBytesRead
            );

            [DllImport("kernel32.dll")]
            static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr hObject);

            [StructLayout(LayoutKind.Sequential)]
            private struct SYSTEM_INFO
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

            [DllImport("kernel32")]
            private static extern void GetSystemInfo(ref SYSTEM_INFO pSI);

            private static uint minaddress;

            private static uint maxaddress;

            private static uint pagesize;

            static MegaDumperPrivate()
            {
                minaddress = 0;
                maxaddress = int.MaxValue;
                pagesize = 0x1000;
                SYSTEM_INFO pSI = new SYSTEM_INFO();
                GetSystemInfo(ref pSI);
                minaddress = pSI.lpMinimumApplicationAddress;
                maxaddress = pSI.lpMaximumApplicationAddress;
                pagesize = pSI.dwPageSize;
            }

            private static int RVA2Offset(byte[] input, int rva)
            {
                int PEOffset = BitConverter.ToInt32(input, 0x3C);
                int nrofsection = BitConverter.ToInt16(input, PEOffset + 0x06);

                for (int i = 0; i < nrofsection; i++)
                {
                    int virtualAddress = BitConverter.ToInt32(input, PEOffset + 0x0F8 + 0x28 * i + 012);
                    int fvirtualsize = BitConverter.ToInt32(input, PEOffset + 0x0F8 + 0x28 * i + 08);
                    int frawAddress = BitConverter.ToInt32(input, PEOffset + 0x28 * i + 0x0F8 + 20);
                    if ((virtualAddress <= rva) && (virtualAddress + fvirtualsize >= rva))
                        return (frawAddress + (rva - virtualAddress));
                }

                return -1;
            }


            private static int Offset2RVA(byte[] input, int offset)
            {
                int PEOffset = BitConverter.ToInt32(input, 0x3C);
                int nrofsection = BitConverter.ToInt16(input, PEOffset + 0x06);

                for (int i = 0; i < nrofsection; i++)
                {
                    int virtualAddress = BitConverter.ToInt32(input, PEOffset + 0x0F8 + 0x28 * i + 012);
                    int virtualsize = BitConverter.ToInt32(input, PEOffset + 0x0F8 + 0x28 * i + 08);
                    int frawAddress = BitConverter.ToInt32(input, PEOffset + 0x28 * i + 0x0F8 + 20);
                    int frawsize = BitConverter.ToInt32(input, PEOffset + 0x28 * i + 0x0F8 + 16);
                    if ((frawAddress <= offset) && (frawAddress + frawsize >= offset))
                        return (virtualAddress + (offset - frawAddress));
                }

                return -1;
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
            };

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

            private static bool FixImportandEntryPoint(int dumpVA, byte[] Dump)
            {
                if (Dump == null || Dump.Length == 0) return false;

                int PEOffset = BitConverter.ToInt32(Dump, 0x3C);

                int ImportDirectoryRva = BitConverter.ToInt32(Dump, PEOffset + 0x080);
                int impdiroffset = RVA2Offset(Dump, ImportDirectoryRva);
                if (impdiroffset == -1) return false;

                byte[] mscoreeAscii = { 0x6D, 0x73, 0x63, 0x6F, 0x72, 0x65, 0x65, 0x2E, 0x64, 0x6C, 0x6C, 0x00 };
                byte[] CorExeMain = { 0x5F, 0x43, 0x6F, 0x72, 0x45, 0x78, 0x65, 0x4D, 0x61, 0x69, 0x6E, 0x00 };
                byte[] CorDllMain = { 0x5F, 0x43, 0x6F, 0x72, 0x44, 0x6C, 0x6C, 0x4D, 0x61, 0x69, 0x6E, 0x00 };
                int ThunkToFix = 0;
                int ThunkData = 0;

                byte[] NameKeeper = new byte[mscoreeAscii.Length];
                int current = 0;
                int NameRVA = BitConverter.ToInt32(Dump, impdiroffset + current + 12);
                while (NameRVA > 0)
                {
                    int NameOffset = RVA2Offset(Dump, NameRVA);
                    if (NameOffset > 0)
                    {
                        try
                        {
                            bool ismscoree = true;
                            for (int i = 0; i < mscoreeAscii.Length; i++)
                            {
                                if (Dump[NameOffset + i] != mscoreeAscii[i])
                                {
                                    ismscoree = false;
                                    break;
                                }
                            }

                            if (ismscoree)
                            {
                                int OriginalFirstThunk = BitConverter.ToInt32(Dump, impdiroffset + current);
                                int OriginalFirstThunkfo = RVA2Offset(Dump, OriginalFirstThunk);
                                if (OriginalFirstThunkfo > 0)
                                {
                                    ThunkData = BitConverter.ToInt32(Dump, OriginalFirstThunkfo);
                                    int ThunkDatafo = RVA2Offset(Dump, ThunkData);
                                    if (ThunkDatafo > 0)
                                    {
                                        ismscoree = true;
                                        for (int i = 0; i < mscoreeAscii.Length; i++)
                                        {
                                            if (Dump[ThunkDatafo + 2 + i] != CorExeMain[i] && Dump[ThunkDatafo + 2 + i] != CorDllMain[i])
                                            {
                                                ismscoree = false;
                                                break;
                                            }
                                        }

                                        if (ismscoree)
                                        {
                                            ThunkToFix = BitConverter.ToInt32(Dump, impdiroffset + current + 16);  // FirstThunk;
                                            break;
                                        }

                                    }
                                }

                            }
                        }
                        catch
                        {
                        }

                    }

                    try
                    {
                        current = current + 20; // 20 = size of IMAGE_IMPORT_DESCRIPTOR
                        NameRVA = BitConverter.ToInt32(Dump, ImportDirectoryRva + current + 12);
                    }
                    catch
                    {
                        break;
                    }
                }

                if (ThunkToFix <= 0 || ThunkData == 0) return false;

                int ThunkToFixfo = RVA2Offset(Dump, ThunkToFix);
                if (ThunkToFixfo < 0) return false;

                BinaryWriter writer = new BinaryWriter(new MemoryStream(Dump));
                int ThunkValue = BitConverter.ToInt32(Dump, ThunkToFixfo);  // old thunk value
                if (ThunkValue <= 0 || RVA2Offset(Dump, ThunkValue) < 0)
                {
                    writer.BaseStream.Position = ThunkToFixfo;
                    writer.Write(ThunkData);
                }

                int EntryPoint = BitConverter.ToInt32(Dump, PEOffset + 0x028);
                if (EntryPoint <= 0 || RVA2Offset(Dump, EntryPoint) < 0)
                {
                    byte[] ThunkToFixbytes = BitConverter.GetBytes(ThunkToFix + dumpVA);
                    for (int i = 0; i < Dump.Length - 6; i++)
                    {
                        if (Dump[i + 0] == 0x0FF && Dump[i + 1] == 0x025 && Dump[i + 2] == ThunkToFixbytes[0] && Dump[i + 3] == ThunkToFixbytes[1] && Dump[i + 4] == ThunkToFixbytes[2] && Dump[i + 5] == ThunkToFixbytes[3])
                        {
                            int EntrPointRVA = Offset2RVA(Dump, i);
                            writer.BaseStream.Position = PEOffset + 0x028;
                            writer.Write(EntrPointRVA);
                            break;
                        }
                    }
                }

                writer.Close();
                return true;
            }

            private const uint PROCESS_VM_OPERATION = 0x0008;
            private const uint PROCESS_VM_READ = 0x0010;
            private const uint PROCESS_VM_WRITE = 0x0020;
            private const uint PROCESS_QUERY_INFORMATION = 0x0400;

            public static unsafe int DumpProcess(uint processId, string DirectoryName)
            {
                IntPtr hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, 0, processId);
                if (hProcess == IntPtr.Zero)
                    return 0;
                try
                {
                    int CurrentCount = 1;
                    bool isok;
                    byte[] onepage = new byte[pagesize];
                    uint BytesRead = 0;
                    byte[] infokeep = new byte[8];
                    MegaDumpDirectoryHelper.CreateDirectories(DirectoryName);
                    for (uint j = minaddress; j < maxaddress; j += pagesize)
                    {

                        isok = ReadProcessMemory(hProcess, j, onepage, pagesize, ref BytesRead);

                        if (isok)
                        {
                            for (int k = 0; k < onepage.Length - 2; k++)
                            {

                                if (onepage[k] == 0x4D && onepage[k + 1] == 0x5A)
                                {
                                    if (ReadProcessMemory(hProcess, (uint)(j + k + 0x03C), infokeep, 4, ref BytesRead))
                                    {
                                        int PEOffset = BitConverter.ToInt32(infokeep, 0);
                                        if (PEOffset > 0 && (PEOffset + 0x0120) < pagesize)
                                        {
                                            if (ReadProcessMemory(hProcess, (uint)(j + k + PEOffset), infokeep, 2, ref BytesRead))
                                            {
                                                if (infokeep[0] == 0x050 && infokeep[1] == 0x045)
                                                {
                                                    long NetMetadata = 0;
                                                    if (ReadProcessMemory(hProcess, (uint)(j + k + PEOffset + 0x0E8), infokeep, 8, ref BytesRead))
                                                        NetMetadata = BitConverter.ToInt64(infokeep, 0);

                                                    #region Dump Native
                                                    if (NetMetadata != 0)
                                                    {
                                                        byte[] PeHeader = new byte[pagesize];
                                                        if (ReadProcessMemory(hProcess, (uint)(j + k), PeHeader, pagesize, ref BytesRead))
                                                        {
                                                            int nrofsection = BitConverter.ToInt16(PeHeader, PEOffset + 0x06);
                                                            if (nrofsection > 0)
                                                            {
                                                                int sectionalignment = BitConverter.ToInt32(PeHeader, PEOffset + 0x038);
                                                                int filealignment = BitConverter.ToInt32(PeHeader, PEOffset + 0x03C);
                                                                short sizeofoptionalheader = BitConverter.ToInt16(PeHeader, PEOffset + 0x014);

                                                                bool IsDll = false;
                                                                if ((PeHeader[PEOffset + 0x017] & 32) != 0) IsDll = true;
                                                                IntPtr pointer = IntPtr.Zero;
                                                                IMAGE_SECTION_HEADER[] sections = new IMAGE_SECTION_HEADER[nrofsection];
                                                                uint ptr = (uint)(j + k + PEOffset) + (uint)sizeofoptionalheader + 4 +
                                                                    (uint)Marshal.SizeOf(typeof(IMAGE_FILE_HEADER));

                                                                for (int i = 0; i < nrofsection; i++)
                                                                {
                                                                    byte[] datakeeper = new byte[Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER))];
                                                                    ReadProcessMemory(hProcess, ptr, datakeeper, (uint)datakeeper.Length, ref BytesRead);
                                                                    fixed (byte* p = datakeeper)
                                                                    {
                                                                        pointer = (IntPtr)p;
                                                                    }

                                                                    sections[i] = (IMAGE_SECTION_HEADER)Marshal.PtrToStructure(pointer, typeof(IMAGE_SECTION_HEADER));
                                                                    ptr = ptr + (uint)Marshal.SizeOf(typeof(IMAGE_SECTION_HEADER));
                                                                }



                                                                // get total raw size (of all sections):
                                                                int totalrawsize = 0;
                                                                int rawsizeoflast = sections[nrofsection - 1].size_of_raw_data;
                                                                int rawaddressoflast = sections[nrofsection - 1].pointer_to_raw_data;
                                                                if (rawsizeoflast > 0 && rawaddressoflast > 0)
                                                                    totalrawsize = rawsizeoflast + rawaddressoflast;
                                                                string filename = "";

                                                                // calculate right size of image
                                                                int actualsizeofimage = BitConverter.ToInt32(PeHeader, PEOffset + 0x050);
                                                                int sizeofimage = actualsizeofimage;
                                                                int calculatedimagesize = BitConverter.ToInt32(PeHeader, PEOffset + 0x0F8 + 012);
                                                                int rawsize, rawAddress, virtualsize, virtualAddress = 0;
                                                                int calcrawsize = 0;

                                                                for (int i = 0; i < nrofsection; i++)
                                                                {
                                                                    virtualsize = sections[i].virtual_size;
                                                                    int toadd = (virtualsize % sectionalignment);
                                                                    if (toadd != 0) toadd = sectionalignment - toadd;
                                                                    calculatedimagesize = calculatedimagesize + virtualsize + toadd;
                                                                }

                                                                if (calculatedimagesize > sizeofimage) sizeofimage = calculatedimagesize;

                                                                try
                                                                {
                                                                    byte[] crap = new byte[totalrawsize];
                                                                }
                                                                catch
                                                                {
                                                                    totalrawsize = sizeofimage;
                                                                }

                                                                if (totalrawsize != 0)
                                                                {
                                                                    try
                                                                    {
                                                                        byte[] rawdump = new byte[totalrawsize];
                                                                        isok = ReadProcessMemory(hProcess, (uint)(j + k), rawdump, (uint)rawdump.Length, ref BytesRead);
                                                                        if (isok)
                                                                        {
                                                                            filename = Path.Combine(DirectoryName, "rawdump_" + (j + k).ToString("X8"));
                                                                            if (File.Exists(filename))
                                                                                filename = Path.Combine(DirectoryName, "rawdump" + CurrentCount.ToString() + "_" + (j + k).ToString("X8"));
                                                                            if (IsDll)
                                                                                filename = filename + ".dll";
                                                                            else
                                                                                filename = filename + ".exe";

                                                                            File.WriteAllBytes(filename, rawdump);
                                                                            CurrentCount++;
                                                                        }
                                                                    }
                                                                    catch
                                                                    {
                                                                    }
                                                                }



                                                                byte[] virtualdump = new byte[sizeofimage];
                                                                Array.Copy(PeHeader, virtualdump, pagesize);

                                                                int rightrawsize = 0;
                                                                for (int l = 0; l < nrofsection; l++)
                                                                {
                                                                    rawsize = sections[l].size_of_raw_data;
                                                                    rawAddress = sections[l].pointer_to_raw_data;
                                                                    virtualsize = sections[l].virtual_size;
                                                                    virtualAddress = sections[l].virtual_address;

                                                                    // RawSize = Virtual Size rounded on FileAlligment
                                                                    calcrawsize = 0;
                                                                    calcrawsize = virtualsize % filealignment;
                                                                    if (calcrawsize != 0) calcrawsize = filealignment - calcrawsize;
                                                                    calcrawsize = virtualsize + calcrawsize;

                                                                    if (calcrawsize != 0 && rawsize != calcrawsize && rawsize != virtualsize
                                                                       || rawAddress < 0)
                                                                    {
                                                                        // if raw size is bad:
                                                                        rawsize = virtualsize;
                                                                        rawAddress = virtualAddress;
                                                                        BinaryWriter writer = new BinaryWriter(new MemoryStream(virtualdump));
                                                                        writer.BaseStream.Position = PEOffset + 0x0F8 + 0x28 * l + 16;
                                                                        writer.Write(virtualsize);
                                                                        writer.BaseStream.Position = PEOffset + 0x0F8 + 0x28 * l + 20;
                                                                        writer.Write(virtualAddress);
                                                                        writer.Close();
                                                                    }

                                                                    byte[] csection = new byte[0];
                                                                    try
                                                                    {
                                                                        csection = new byte[rawsize];
                                                                    }
                                                                    catch
                                                                    {
                                                                        csection = new byte[virtualsize];
                                                                    }
                                                                    int rightsize = csection.Length;
                                                                    isok = ReadProcessMemory(hProcess, (uint)(j + k + virtualAddress), csection, (uint)rawsize, ref BytesRead);
                                                                    if (!isok || BytesRead != rawsize)
                                                                    {
                                                                        rightsize = 0;
                                                                        byte[] currentpage = new byte[pagesize];
                                                                        for (int c = 0; c < rawsize; c = c + (int)pagesize)
                                                                        {
                                                                            // some section have a houge size so : try
                                                                            try
                                                                            {
                                                                                isok = ReadProcessMemory(hProcess, (uint)(j + k + virtualAddress + c), currentpage, pagesize, ref BytesRead);
                                                                            }
                                                                            catch
                                                                            {
                                                                                break;
                                                                            }

                                                                            if (isok)
                                                                            {
                                                                                rightsize = rightsize + (int)pagesize;
                                                                                for (int i = 0; i < pagesize; i++)
                                                                                {
                                                                                    if ((c + i) < csection.Length)
                                                                                        csection[c + i] = currentpage[i];
                                                                                }
                                                                            }


                                                                        }
                                                                    }


                                                                    try
                                                                    {
                                                                        Array.Copy(csection, 0, virtualdump, rawAddress, rightsize);
                                                                    }
                                                                    catch
                                                                    {
                                                                    }

                                                                    if (l == nrofsection - 1)
                                                                    {
                                                                        rightrawsize = rawAddress + rawsize;
                                                                    }

                                                                }
                                                                FixImportandEntryPoint((int)(j + k), virtualdump);
                                                                filename = Path.Combine(DirectoryName, "vdump_" + (j + k).ToString("X8"));
                                                                if (File.Exists(filename))
                                                                    filename = Path.Combine(DirectoryName, "vdump" + CurrentCount.ToString() + "_" + (j + k).ToString("X8"));
                                                                if (IsDll)
                                                                    filename = filename + ".dll";
                                                                else
                                                                    filename = filename + ".exe";
                                                                using (var fout = new FileStream(filename, FileMode.Create))
                                                                    fout.Write(virtualdump, 0, Math.Min(rightrawsize, virtualdump.Length));
                                                                CurrentCount++;
                                                            }
                                                        }
                                                        // dumping end here
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
                    // rename files:
                    foreach (FileInfo fi in new DirectoryInfo(DirectoryName).GetFiles())
                    {
                        FileVersionInfo info = FileVersionInfo.GetVersionInfo(fi.FullName);
                        if (info.OriginalFilename != null && info.OriginalFilename != "")
                        {
                            string Newfilename = Path.Combine(DirectoryName, info.OriginalFilename);
                            int count = 2;
                            if (File.Exists(Newfilename))
                            {
                                string extension = Path.GetExtension(Newfilename);
                                if (extension == "") extension = ".dll";
                                do
                                {
                                    Newfilename = Path.Combine(DirectoryName, Path.GetFileNameWithoutExtension(info.OriginalFilename) + "(" + count.ToString() + ")" + extension);
                                    count++;
                                }
                                while (File.Exists(Newfilename));
                            }

                            File.Move(fi.FullName, Newfilename);
                        }
                    }
                    MegaDumpDirectoryHelper.Classify(DirectoryName);
                    CurrentCount--;
                    return CurrentCount;

                }
                finally
                {
                    CloseHandle(hProcess);
                }
            }
        }
    }
}
