using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using FastWin32.Diagnostics;
using FastWin32.Memory;
using Microsoft.Diagnostics.Runtime;

namespace ExtremeDumper.InjectingDumper
{
    public class DumperInjector : IDumper
    {
        private uint _processId;

        private static readonly byte[] InjectingDumper2 = GetInjectingDumper(2);

        private static readonly byte[] InjectingDumper4 = GetInjectingDumper(4);

        public DumperInjector(uint processId) => _processId = processId;

        public bool DumpModule(IntPtr moduleHandle, string filePath) => DumpModulePrivate(moduleHandle, Path.GetDirectoryName(filePath));

        private bool DumpModulePrivate(IntPtr moduleHandle, string directoryPath)
        {
            ClrModule clrModule;
            (uint Rva, uint Size) metadataDictionary;
            int ret;

            clrModule = GetModule(moduleHandle);
            if (clrModule == null)
                return false;
            metadataDictionary = GetMetadataDictionary(clrModule);
            return Injector.InjectManaged(_processId, UnpackDumper(clrModule.Runtime.ClrInfo.Version.Major), "InjectingDumper.Dumper", "TryDumpModule", Convert.ToBase64String(Encoding.Unicode.GetBytes($"{((ulong)moduleHandle).ToString()}|{metadataDictionary.Rva.ToString()}|{metadataDictionary.Size.ToString()}|{directoryPath}")), out ret) && ret == 1;
        }

        private ClrModule GetModule(IntPtr moduleHandle)
        {
            DataTarget dataTarget;

            using (dataTarget = DataTarget.AttachToProcess((int)_processId, 10000, AttachFlag.Passive))
                foreach (ClrInfo clrInfo in dataTarget.ClrVersions)
                    foreach (ClrModule clrModule in clrInfo.CreateRuntime().Modules)
                        if ((IntPtr)clrModule.ImageBase == moduleHandle)
                        {
                            dataTarget.SelectedModule = clrModule;
                            return clrModule;
                        }
            dataTarget.SelectedModule = null;
            return null;
        }

        public unsafe (uint Rva, uint Size) GetMetadataDictionary(ClrModule module)
        {
            IntPtr moduleHandle;
            ulong metadataAddress;
            ulong metadataLength;
            (uint Rva, uint Size) result;

            moduleHandle = (IntPtr)module.ImageBase;
            metadataAddress = module.MetadataAddress;
            metadataLength = module.MetadataLength;
            if (metadataAddress != 0)
                return ((uint)(metadataAddress - (ulong)moduleHandle), (uint)metadataLength);
            result = (0, 0);
            MemoryIO.EnumPages(_processId, moduleHandle, pageInfo =>
            {
                byte[] pageBuffer;

                Debug.Assert(pageInfo.Address == moduleHandle);
                pageBuffer = new byte[pageInfo.Size];
                if (!MemoryIO.ReadBytes(_processId, pageInfo.Address, pageBuffer))
                {
                    Debug.Assert(false);
                    return false;
                }
                fixed (byte* p = &pageBuffer[0])
                    for (int i = 0; i < pageBuffer.Length / 4; i++)
                        if (((int*)p)[i] == 0x424A5342)
                        {
                            result = ((uint)((ulong)pageInfo.Address - (ulong)moduleHandle + (ulong)i * 4), (uint)metadataLength);
                            return false;
                        }
                return true;
            });
            return result;
        }

        public int DumpProcess(string directoryPath)
        {
            DataTarget dataTarget;
            int count;

            count = 0;
            using (dataTarget = DataTarget.AttachToProcess((int)_processId, 10000, AttachFlag.Passive))
                foreach (ClrInfo clrInfo in dataTarget.ClrVersions)
                    foreach (ClrModule clrModule in clrInfo.CreateRuntime().Modules)
                    {
                        dataTarget.SelectedModule = clrModule;
                        if (DumpModulePrivate((IntPtr)clrModule.ImageBase, directoryPath))
                            count++;
                    }
            return count;
        }

        private static byte[] GetInjectingDumper(int clrVersion)
        {
            BinaryReader binaryReader;

            using (binaryReader = new BinaryReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"ExtremeDumper.InjectingDumper.InjectingDumper{clrVersion.ToString()}.dll")))
                return binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
        }

        private string UnpackDumper(int clrVersion)
        {
            string path;

            path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".dll");
            switch (clrVersion)
            {
                case 2:
                    File.WriteAllBytes(path, InjectingDumper2);
                    break;
                case 4:
                    File.WriteAllBytes(path, InjectingDumper4);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return path;
        }
    }
}
