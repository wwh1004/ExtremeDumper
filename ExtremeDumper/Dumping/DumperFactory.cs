using System;

namespace ExtremeDumper.Dumping;

public enum DumperType {
	Normal,
	AntiAntiDump
}

public static class DumperFactory {
	public static IDumper Create(uint processId, DumperType dumperType) {
		switch (dumperType) {
		case DumperType.Normal:
			return new NormalDumper(processId);
		case DumperType.AntiAntiDump:
			return new AntiAntiDumper(processId);
		default:
			throw new ArgumentOutOfRangeException(nameof(dumperType));
		}
	}
}
