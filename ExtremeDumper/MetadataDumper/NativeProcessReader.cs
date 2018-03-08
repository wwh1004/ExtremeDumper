using System;
using dndbg.DotNet;
using FastWin32.Memory;

namespace ExtremeDumper.MetadataDumper
{
    internal class NativeProcessReader : IProcessReader
    {
        private uint _processId;

        public NativeProcessReader(uint processId) => _processId = processId;

        public int ReadBytes(ulong address, byte[] data, int index, int count)
        {
            byte[] buffer;
            uint size;

            buffer = new byte[count];
            MemoryIO.ReadBytes(_processId, (IntPtr)address, buffer, out size);
            Buffer.BlockCopy(buffer, 0, data, index, count);
            return (int)size;
        }
    }
}
