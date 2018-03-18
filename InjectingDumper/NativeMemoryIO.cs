using System;
using static InjectingDumper.NativeMethods;

namespace InjectingDumper
{
    internal class NativeMemoryIO
    {
        public IntPtr BaseAddress { get; set; }

        public long Position { get; set; }

        public NativeMemoryIO(IntPtr baseAddress) => BaseAddress = baseAddress;

        public unsafe uint ReadUInt32()
        {
            uint value;

            ReadProcessMemory(CURRENT_PROCESS, (IntPtr)((ulong)BaseAddress + (ulong)Position), out value, 4, null);
            Position += 4;
            return value;
        }

        public unsafe void WriteUInt32(uint value)
        {
            WriteProcessMemory(CURRENT_PROCESS, (IntPtr)((ulong)BaseAddress + (ulong)Position), ref value, 4, null);
            Position += 4;
        }
    }
}
