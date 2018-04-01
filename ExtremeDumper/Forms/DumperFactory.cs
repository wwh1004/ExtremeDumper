using System.ComponentModel;
using ExtremeDumper.Dumper;

namespace ExtremeDumper.Forms
{
    public static class DumperFactory
    {
        public static IDumper GetDumper(uint processId, DumperCore dumperCore)
        {
            switch (dumperCore)
            {
                case DumperCore.MegaDumper:
                    return MegaDumper.Create(processId);
                case DumperCore.MetaDumper:
                    return MetaDumper.Create(processId);
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}
