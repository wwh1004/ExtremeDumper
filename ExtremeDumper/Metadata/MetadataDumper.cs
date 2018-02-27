using System;

namespace ExtremeDumper.Metadata
{
    public abstract class MetadataDumper : IDumper
    {
        public abstract bool DumpModule(IntPtr moduleHandle, string filePath);

        public abstract int DumpProcess(string directoryPath);
    }
}
