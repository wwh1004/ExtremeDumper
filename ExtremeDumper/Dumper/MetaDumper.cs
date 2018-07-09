using System;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using FastWin32.Memory;
using Microsoft.Diagnostics.Runtime;
using static ExtremeDumper.Dumper.NativeMethods;
using size_t = System.IntPtr;

namespace ExtremeDumper.Dumper
{
    public sealed unsafe class MetaDumper : IDumper
    {
        private uint _processId;

        private SafeNativeHandle _processHandle;

        private bool _is64;

        private bool _isDisposed;

        private MetaDumper()
        {
        }

        public static IDumper Create(uint processId)
        {
            IntPtr processHandle;

            processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, processId);
            return processHandle == IntPtr.Zero
                ? null
                : new MetaDumper
                {
                    _processId = processId,
                    _processHandle = processHandle,
                    _is64 = Is64BitProcess(processHandle)
                };
        }

        public bool DumpModule(IntPtr moduleHandle, string filePath)
        {
            byte[] assembly;
            ClrModule clrModule;
            ModuleDef moduleDef;

            assembly = RemotePEImageCopyer.Copy(_processId, _processHandle, moduleHandle, _is64);
            clrModule = GetClrModule(moduleHandle);
            fixed (byte* p = assembly)
            {
                try
                {
                    moduleDef = ModuleDefMD.Load((IntPtr)p);
                    moduleDef.Write(filePath, new ModuleWriterOptions(moduleDef) { MetadataOptions = new MetadataOptions(MetadataFlags.KeepOldMaxStack) });
                }
                catch
                {
                    FixMDD(p, (uint)(clrModule.MetadataAddress - (ulong)moduleHandle), (uint)clrModule.MetadataLength);
                    moduleDef = ModuleDefMD.Load((IntPtr)p);
                    moduleDef.Write(filePath, new ModuleWriterOptions(moduleDef) { MetadataOptions = new MetadataOptions(MetadataFlags.KeepOldMaxStack) });
                }
            }
            return true;
        }

        private void FixMDD(byte* p, uint mdRva, uint mdSize)
        {
            //Fix .Net MetaData Directory
            Memory memory;
            uint peOffset;
            uint mddRva;

#pragma warning disable IDE0017
            memory = new Memory(p);
#pragma warning restore IDE0017
            memory.Position = 0x3C;
            peOffset = memory.ReadUInt32();
            memory.Position = peOffset + (_is64 ? 0xF8 : 0xE8);
            mddRva = (uint)memory.Position + 4;
            memory.WriteUInt32(mddRva);
            //.Net MetaData Directory RVA
            for (int i = 0; i < 72; i += 4)
            {
                memory.Position = mddRva + i;
                switch (i)
                {
                    case 0:
                        //cb
                        memory.WriteUInt32(0x48);
                        break;
                    case 4:
                        //MajorRuntimeVersion
                        //MinorRuntimeVersion
                        memory.WriteUInt32(2);
                        break;
                    case 8:
                        //MetaData RVA
                        memory.WriteUInt32(mdRva);
                        break;
                    case 12:
                        //MetaData Size
                        memory.WriteUInt32(mdSize);
                        break;
                    case 16:
                        //Flags
                        memory.WriteUInt32(0);
                        break;
                    case 20:
                        //EntryPointTokenOrRVA
                        memory.WriteUInt32(0x6000004);
                        //随便写一个
                        break;
                    default:
                        memory.WriteUInt32(0);
                        break;
                }
            }
        }

        public int DumpProcess(string directoryPath)
        {
            DataTarget dataTarget;
            int result = 0;

            using (dataTarget = DataTarget.AttachToProcess((int)_processId, 10000, AttachFlag.Passive))
                foreach (ClrInfo clrInfo in dataTarget.ClrVersions)
                    foreach (ClrModule clrModule in clrInfo.CreateRuntime().Modules)
                    {
                        try
                        {
                            string moduleName;

                            moduleName = clrModule.Name ?? "EmptyName";
                            moduleName = clrModule.IsDynamic ? moduleName.Split(',')[0] : Path.GetFileName(moduleName);
                            if (DumpModule((IntPtr)clrModule.ImageBase, Path.Combine(directoryPath, moduleName)))
                                result++;
                        }
                        catch
                        {
                        }
                    }
            return result;
        }

        private ClrModule GetClrModule(IntPtr moduleHandle)
        {
            DataTarget dataTarget;

            using (dataTarget = DataTarget.AttachToProcess((int)_processId, 10000, AttachFlag.Passive))
                foreach (ClrInfo clrInfo in dataTarget.ClrVersions)
                    foreach (ClrModule clrModule in clrInfo.CreateRuntime().Modules)
                        if ((IntPtr)clrModule.ImageBase == moduleHandle)
                            return clrModule;
            return null;
        }

        private static bool Is64BitProcess(IntPtr processHandle)
        {
            bool isWow64;

            if (!EnvironmentCache.Is64BitOperatingSystem)
                return false;
            IsWow64Process(processHandle, out isWow64);
            return !isWow64;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _processHandle.Dispose();
            _isDisposed = true;
        }

        #region private class
        private class Memory
        {
            public byte* BaseAddress { get; set; }

            public long Position { get; set; }

            public Memory(byte* baseAddress) => BaseAddress = baseAddress;

            public uint ReadUInt32()
            {
                uint value;

                value = *(uint*)(BaseAddress + Position);
                Position += 4;
                return value;
            }

            public void WriteUInt32(uint value)
            {
                *(uint*)(BaseAddress + Position) = value;
                Position += 4;
            }
        }

        private static class RemotePEImageCopyer
        {
            public static byte[] Copy(uint processId, IntPtr processHandle, IntPtr moduleHandle, bool is64)
            {
                byte[] buffer;

                buffer = new byte[GetImageSize(processHandle, moduleHandle, is64)];
                MemoryIO.EnumPages(processId, moduleHandle, pageInfo =>
                {
                    int startOffset;
                    int endOffset;

                    startOffset = (int)((ulong)pageInfo.Address - (ulong)moduleHandle);
                    //以p为起点，远程进程中页面起点映射到buffer中的偏移
                    endOffset = startOffset + (int)pageInfo.Size;
                    //以p为起点，远程进程中页面终点映射到buffer中的偏移
                    fixed (byte* p = buffer)
                        if (startOffset < 0)
                        {
                            //页面前半部分超出buffer
                            ReadProcessMemory(processHandle, moduleHandle, p, (size_t)((ulong)pageInfo.Size - ((ulong)moduleHandle - (ulong)pageInfo.Address)), null);
                            return true;
                        }
                        else
                        {
                            if (endOffset <= buffer.Length)
                            {
                                //整个页面都可以存入buffer
                                ReadProcessMemory(processHandle, pageInfo.Address, p + startOffset, pageInfo.Size, null);
                                return true;
                            }
                            else
                            {
                                //页面后半部分/全部超出buffer
                                ReadProcessMemory(processHandle, pageInfo.Address, p + startOffset, pageInfo.Size - (endOffset - buffer.Length), null);
                                return false;
                            }
                        }
                });
                return buffer;
            }

            private static ulong GetImageSize(IntPtr processHandle, IntPtr moduleHandle, bool is64)
            {
                int pagesize = (int)EnvironmentCache.SystemInfo.dwPageSize;
                bool isok;

                byte[] InfoKeep = new byte[8];
                size_t BytesRead = size_t.Zero;

                int nrofsection = 0;
                int filealignment = 0;
                int rawaddress;
                int offset = 0;
                int sectionsoffset = is64 ? 0x108 : 0xF8;

                isok = ReadProcessMemory(processHandle, moduleHandle + 0x03C, InfoKeep, (size_t)4, &BytesRead);
                int PEOffset = BitConverter.ToInt32(InfoKeep, 0);

                isok = ReadProcessMemory(processHandle, moduleHandle + PEOffset + sectionsoffset + +20, InfoKeep, (size_t)4, &BytesRead);
                byte[] PeHeader = new byte[pagesize];

                rawaddress = BitConverter.ToInt32(InfoKeep, 0);
                int sizetocopy = rawaddress;
                if (sizetocopy > pagesize) sizetocopy = pagesize;
                isok = ReadProcessMemory(processHandle, moduleHandle, PeHeader, (size_t)sizetocopy, &BytesRead);
                offset = offset + rawaddress;

                nrofsection = BitConverter.ToInt16(PeHeader, PEOffset + 0x06);
                int sectionalignment = BitConverter.ToInt32(PeHeader, PEOffset + 0x038);
                filealignment = BitConverter.ToInt32(PeHeader, PEOffset + 0x03C);

                int sizeofimage = BitConverter.ToInt32(PeHeader, PEOffset + 0x050);

                int calculatedimagesize = BitConverter.ToInt32(PeHeader, PEOffset + sectionsoffset + 012);

                for (int i = 0; i < nrofsection; i++)
                {
                    int virtualsize = BitConverter.ToInt32(PeHeader, PEOffset + sectionsoffset + 0x28 * i + 08);
                    int toadd = (virtualsize % sectionalignment);
                    if (toadd != 0) toadd = sectionalignment - toadd;
                    calculatedimagesize = calculatedimagesize + virtualsize + toadd;
                }

                if (calculatedimagesize > sizeofimage) sizeofimage = calculatedimagesize;
                return (ulong)sizeofimage;
            }
        }
        #endregion
    }
}
