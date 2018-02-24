using System;

namespace ExtremeDumper.Metadata
{
    public abstract class MetadataDumper : IDumper
    {
        public bool DumpModule(IntPtr moduleHandle, string path)
        {
            throw new NotSupportedException();
        }

        public bool DumpModule(uint moduleId, string path)
        {
            throw new NotImplementedException();
        }

        public int DumpProcess(string path)
        {
            throw new NotImplementedException();
        }
    }
}
