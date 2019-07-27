using System.ComponentModel;
using ExtremeDumper.Dumper;

namespace ExtremeDumper.Forms {
	public static class DumperFactory {
		public static IDumper GetDumper(uint processId, DumperType dumperType) {
			switch (dumperType) {
			case DumperType.MegaDumper:
				return MegaDumper.Create(processId);
			case DumperType.AntiAntiDumper:
				return AntiAntiDumper.Create(processId);
			default:
				throw new InvalidEnumArgumentException();
			}
		}
	}
}
