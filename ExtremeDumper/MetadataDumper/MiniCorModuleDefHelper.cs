using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using dndbg.COM.MetaData;
using dndbg.DotNet;
using dndbg.Engine;
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

        public unsafe IBinaryReader CreateBodyReader(uint bodyRva, uint mdToken)
        {
            uint* pClass;
            string szMethod;
            uint cchMethod = 256;
            uint* pchMethod;
            uint* pdwAttr;
            ushort** ppvSigBlob;
            uint* pcbSigBlob;
            uint* pulCodeRVA;
            uint* pdwImplFlags;
            int hr;

            hr = GetMethodProps(_mdi, mdToken, out pClass, out szMethod, cchMethod, out pchMethod, out pdwAttr, out ppvSigBlob, out pcbSigBlob, out pulCodeRVA, out pdwImplFlags);
            if (IsDynamic)
            {
            }
            return new ProcessBinaryReader(new NativeProcessReader(_processId), _clrModule.ImageBase + bodyRva);

            //// bodyRva can be 0 if it's a dynamic module. this.module.Address will also be 0.
            //if (!IsDynamic && bodyRva == 0)
            //    return null;
            ////var func = new DnModule(null,null,0,0,0).CorModule.GetFunctionFromToken(mdToken);
            //var func = new MiniCorFunction();
            //var ilCode = func?.ILCode;
            //if (ilCode == null)
            //    return null;
            //ulong addr = ilCode.Address;
            //if (addr == 0)
            //    return null;

            //Debug.Assert(addr >= FAT_HEADER_SIZE);
            //if (addr < FAT_HEADER_SIZE)
            //    return null;

            //if (IsDynamic)
            //{
            //    // It's always a fat header, see COMDynamicWrite::SetMethodIL() (coreclr/src/vm/comdynamic.cpp)
            //    addr -= FAT_HEADER_SIZE;
            //    var reader = new ProcessBinaryReader(new NativeProcessReader(_processId), 0);
            //    Debug.Assert((reader.Position = (long)addr) == (long)addr);
            //    Debug.Assert((reader.ReadByte() & 7) == 3);
            //    Debug.Assert((reader.Position = (long)addr + 4) == (long)addr + 4);
            //    Debug.Assert(reader.ReadUInt32() == ilCode.Size);
            //    reader.Position = (long)addr;
            //    return reader;
            //}
            //else
            //{
            //    uint codeSize = ilCode.Size;
            //    // The address to the code is returned but we want the header. Figure out whether
            //    // it's the 1-byte or fat header.
            //    var reader = new ProcessBinaryReader(new NativeProcessReader(_processId), 0);
            //    uint locVarSigTok = func.LocalVarSigToken;
            //    bool isBig = codeSize >= 0x40 || (locVarSigTok & 0x00FFFFFF) != 0;
            //    if (!isBig)
            //    {
            //        reader.Position = (long)addr - 1;
            //        byte b = reader.ReadByte();
            //        var type = b & 7;
            //        if ((type == 2 || type == 6) && (b >> 2) == codeSize)
            //        {
            //            // probably small header
            //            isBig = false;
            //        }
            //        else
            //        {
            //            reader.Position = (long)addr - (long)FAT_HEADER_SIZE + 4;
            //            uint headerCodeSize = reader.ReadUInt32();
            //            uint headerLocVarSigTok = reader.ReadUInt32();
            //            bool valid = headerCodeSize == codeSize &&
            //                (locVarSigTok & 0x00FFFFFF) == (headerLocVarSigTok & 0x00FFFFFF) &&
            //                ((locVarSigTok & 0x00FFFFFF) == 0 || locVarSigTok == headerLocVarSigTok);
            //            Debug.Assert(valid);
            //            if (!valid)
            //                return null;
            //            isBig = true;
            //        }
            //    }

            //    reader.Position = (long)addr - (long)(isBig ? FAT_HEADER_SIZE : 1);
            //    return reader;
            //}


            //// bodyRva can be 0 if it's a dynamic module. this.module.Address will also be 0.
            //if (!IsDynamic && bodyRva == 0)
            //    return null;

            //var func = module.CorModule.GetFunctionFromToken(mdToken);
            //var ilCode = func?.ILCode;
            //if (ilCode == null)
            //    return null;
            //ulong addr = ilCode.Address;
            //if (addr == 0)
            //    return null;

            //Debug.Assert(addr >= FAT_HEADER_SIZE);
            //if (addr < FAT_HEADER_SIZE)
            //    return null;

            //if (module.IsDynamic)
            //{
            //    // It's always a fat header, see COMDynamicWrite::SetMethodIL() (coreclr/src/vm/comdynamic.cpp)
            //    addr -= FAT_HEADER_SIZE;
            //    var reader = new ProcessBinaryReader(new CorProcessReader(module.Process), 0);
            //    Debug.Assert((reader.Position = (long)addr) == (long)addr);
            //    Debug.Assert((reader.ReadByte() & 7) == 3);
            //    Debug.Assert((reader.Position = (long)addr + 4) == (long)addr + 4);
            //    Debug.Assert(reader.ReadUInt32() == ilCode.Size);
            //    reader.Position = (long)addr;
            //    return reader;
            //}
            //else
            //{
            //    uint codeSize = ilCode.Size;
            //    // The address to the code is returned but we want the header. Figure out whether
            //    // it's the 1-byte or fat header.
            //    var reader = new ProcessBinaryReader(new CorProcessReader(module.Process), 0);
            //    uint locVarSigTok = func.LocalVarSigToken;
            //    bool isBig = codeSize >= 0x40 || (locVarSigTok & 0x00FFFFFF) != 0;
            //    if (!isBig)
            //    {
            //        reader.Position = (long)addr - 1;
            //        byte b = reader.ReadByte();
            //        var type = b & 7;
            //        if ((type == 2 || type == 6) && (b >> 2) == codeSize)
            //        {
            //            // probably small header
            //            isBig = false;
            //        }
            //        else
            //        {
            //            reader.Position = (long)addr - (long)FAT_HEADER_SIZE + 4;
            //            uint headerCodeSize = reader.ReadUInt32();
            //            uint headerLocVarSigTok = reader.ReadUInt32();
            //            bool valid = headerCodeSize == codeSize &&
            //                (locVarSigTok & 0x00FFFFFF) == (headerLocVarSigTok & 0x00FFFFFF) &&
            //                ((locVarSigTok & 0x00FFFFFF) == 0 || locVarSigTok == headerLocVarSigTok);
            //            Debug.Assert(valid);
            //            if (!valid)
            //                return null;
            //            isBig = true;
            //        }
            //    }

            //    reader.Position = (long)addr - (long)(isBig ? FAT_HEADER_SIZE : 1);
            //    return reader;
            //}
        }

        private static unsafe int GetMethodProps(IMetaDataImport mdi, uint mb, out uint* pClass, out string szMethod, uint cchMethod, out uint* pchMethod, out uint* pdwAttr, out ushort** ppvSigBlob, out uint* pcbSigBlob, out uint* pulCodeRVA, out uint* pdwImplFlags)
        {
            int hr;

            uint a = 0;
            uint b = 0;
            uint c = 0;
            ushort d = 0;
            ushort* dd = &d;
            uint e = 0;
            uint f = 0;
            pClass = &a;
            pchMethod = &b;
            pdwAttr = &c;
            ppvSigBlob = &dd;
            pcbSigBlob = &f;
            pulCodeRVA = &e;
            pdwImplFlags = &f;
            fixed (byte* p = new byte[cchMethod])
            {
                hr = mdi.GetMethodProps(mb, (IntPtr)pClass, (IntPtr)p, cchMethod, (IntPtr)pchMethod, (IntPtr)pdwAttr, (IntPtr)ppvSigBlob, (IntPtr)pcbSigBlob, (IntPtr)pulCodeRVA, (IntPtr)pdwImplFlags);
                szMethod = new string((char*)p);
            }
            return hr;
        }

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
