using System;

namespace ExtremeDumper.Metadata
{
    public class DbgDumper : MetadataDumper
    {
        public override bool DumpModule(IntPtr moduleHandle, string filePath)
        {
            throw new NotImplementedException();
        }

        public override int DumpProcess(string directoryPath)
        {
            throw new NotImplementedException();
        }
    }
}
