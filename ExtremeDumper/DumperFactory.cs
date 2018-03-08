using System;
using System.ComponentModel;
using ExtremeDumper.MegaDumper;
using ExtremeDumper.MetadataDumper;
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
                    return new PassiveDumper(processId);
                    if (!Process32.Is64BitProcess(processId, out is64))
                        throw new Win32Exception();
                    if (is64)
                        return new MegaDumper64(processId);
                    else
                        return new MegaDumper32(processId);
                case DumperCore.PassiveDumper:
                    throw new NotImplementedException();
                //return new PassiveDumper(processId);
                case DumperCore.DbgDumper:
                    return new DbgDumper(processId);
                case DumperCore.ProfDumper:
                    throw new NotImplementedException();
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}
