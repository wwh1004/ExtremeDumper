using System;

namespace ExtremeDumper.Dumping;

public enum DumperType {
	Normal
}

public static class DumperFactory {
	public static IDumper GetDumper(uint processId, DumperType dumperType) {
		switch (dumperType) {
		case DumperType.Normal:
			return NormalDumper.Create(processId);
		default:
			throw new ArgumentOutOfRangeException(nameof(dumperType));
		}
	}
}
