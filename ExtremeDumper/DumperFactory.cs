using System;
using System.ComponentModel;
using ExtremeDumper.MegaDumper;
using ExtremeDumper.Metadata;
using FastWin32.Diagnostics;

namespace ExtremeDumper
{
    public static class DumperFactory
    {
        public static IDumper GetDumper(uint processId, DumperCore dumperCore)
        {
            bool is64;

            switch (dumperCore)
            {
                case DumperCore.MegaDumper:
                    if (!Process32.Is64BitProcess(processId, out is64))
                        throw new Win32Exception();
                    if (is64)
                        return new MegaDumper64(processId);
                    else
                        return new MegaDumper32(processId);
                case DumperCore.MetadataWithDebugger:
                    return new TestDumper(processId);
                    //throw new NotImplementedException();
                case DumperCore.MetadataWithProfiler:
                    throw new NotImplementedException();
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}
