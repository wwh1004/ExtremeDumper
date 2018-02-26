using System;

namespace ExtremeDumper.Metadata
{
    public abstract class MetadataDumper : IDumper
    {
        public bool DumpModule(IntPtr moduleHandle, string filePath)
        {
            throw new NotSupportedException();
        }

        public int DumpProcess(string directoryPath)
        {
            throw new NotImplementedException();
        }
    }
}
