using System;

namespace ExtremeDumper.Dumper {
	public enum DumperType {
		Normal,

		AntiAntiDump
	}

	public static class DumperFactory {
		public static IDumper GetDumper(uint processId, DumperType dumperType) {
			switch (dumperType) {
			case DumperType.Normal:
				return NormalDumper.Create(processId);
			case DumperType.AntiAntiDump:
				return AntiAntiDumper.Create(processId);
			default:
				throw new ArgumentOutOfRangeException(nameof(dumperType));
			}
		}
	}
}
