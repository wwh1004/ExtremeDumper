using ExtremeDumper.AntiAntiDump.Serialization;

namespace ExtremeDumper.AntiAntiDump {
	public static class Injection {
		public static int Main(string arg) {
			try {
				InjectionOptions options;

				options = XmlSerializer.Deserialize<InjectionOptions>(arg);
				new MetadataInfoService().Start(options.PortName, options.ObjectName);
			}
			catch {
				return -1;
			}
			return 0;
		}
	}
}
