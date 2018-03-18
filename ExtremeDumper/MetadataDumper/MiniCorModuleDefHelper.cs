using System;
using System.Diagnostics;
using System.Linq;
using dndbg.COM.MetaData;
using dndbg.DotNet;
using dnlib.DotNet;
using dnlib.IO;
using dnlib.PE;
using FastWin32.Memory;
using Microsoft.Diagnostics.Runtime;

namespace ExtremeDumper.MetadataDumper
{
    internal class MiniCorModuleDefHelper : ICorModuleDefHelper
    {
        private const ulong FAT_HEADER_SIZE = 3 * 4;

        private uint _processId;

        private IMetaDataImport _mdi;

        private ClrModule _clrModule;

        private ImageSectionHeader[] _sectionHeaders;

        public MiniCorModuleDefHelper(uint processId, IMetaDataImport mdi, ClrModule clrModule)
        {
            _processId = processId;
            _mdi = mdi;
            _clrModule = clrModule;
        }

        public IAssembly CorLib => _clrModule.Runtime.ClrInfo.Version.Major == 4 ? AssemblyRefUser.CreateMscorlibReferenceCLR40() : AssemblyRefUser.CreateMscorlibReferenceCLR20();

        public bool IsDynamic => false;

        public bool IsInMemory => _clrModule.IsDynamic;

        public bool? IsCorLib => false;

        public string Filename => IsInMemory ? string.Empty : _clrModule.FileName;

        public bool IsManifestModule { get; set; }

        public unsafe IBinaryReader CreateBodyReader(uint bodyRva, uint mdToken) => new ProcessBinaryReader(new NativeProcessReader(_processId), _clrModule.ImageBase + bodyRva);

        public byte[] ReadFieldInitialValue(uint fieldRva, uint fdToken, int size)
        {
            if (IsDynamic)
                return null;

            return ReadFromRVA(fieldRva, size);
        }

        private byte[] ReadFromRVA(uint rva, int size)
        {
            if (IsDynamic)
                return null;

            ulong addr = _clrModule.ImageBase;
            Debug.Assert(addr != 0);
            if (addr == 0)
                return null;

            var offs = RVAToAddressOffset(rva);
            if (offs == null)
                return null;
            addr += offs.Value;

            byte[] data = new byte[size];
            MemoryIO.ReadBytes(_processId, (IntPtr)_clrModule.ImageBase, data, out uint numOfRead);
            data = data.Take((int)numOfRead).ToArray();
            //var data = module.Process.CorProcess.ReadMemory(addr, size);
            Debug.Assert(data != null && data.Length == size);
            return data;
        }

        private uint? RVAToAddressOffset(uint rva)
        {
            if (IsDynamic)
                return null;
            if (!IsInMemory)
                return rva;
            return RVAToFileOffset(rva);
        }

        private uint? RVAToFileOffset(uint rva)
        {
            foreach (var sh in GetOrCreateSectionHeaders())
            {
                if ((uint)sh.VirtualAddress <= rva && rva < (uint)sh.VirtualAddress + Math.Max(sh.SizeOfRawData, sh.VirtualSize))
                    return rva - (uint)sh.VirtualAddress + sh.PointerToRawData;
            }

            return null;
        }

        private ImageSectionHeader[] GetOrCreateSectionHeaders()
        {
            var h = _sectionHeaders;
            if (h != null)
                return h;

            try
            {
                ulong addr = _clrModule.ImageBase;
                if (addr == 0)
                    return _sectionHeaders = ArrayAddIn.Empty<ImageSectionHeader>();
                var data = new byte[0x1000];
                //module.Process.CorProcess.ReadMemory(module.Address, data, 0, data.Length, out int sizeRead);
                MemoryIO.ReadBytes(_processId, (IntPtr)_clrModule.ImageBase, data);
                using (var peImage = new PEImage(data, !IsDynamic && IsInMemory ? ImageLayout.File : ImageLayout.Memory, true))
                    return _sectionHeaders = peImage.ImageSectionHeaders.ToArray();
            }
            catch
            {
                Debug.Fail("Couldn't read section headers");
            }
            return _sectionHeaders = ArrayAddIn.Empty<ImageSectionHeader>();
        }

        public IImageStream CreateResourceStream(uint offset) => null;
    }
}
