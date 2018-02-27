using System;

namespace ExtremeDumper.Metadata
{
    public class ProfDumper : MetadataDumper
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
