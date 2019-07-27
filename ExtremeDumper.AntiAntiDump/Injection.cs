using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using ExtremeDumper.AntiAntiDump.Serialization;

namespace ExtremeDumper.AntiAntiDump {
	public static class Injection {
		public static int Main(string arg) {
			try {
				InjectionOptions options;
				MetadataInfoService metadataInfoService;

				options = XmlSerializer.Deserialize<InjectionOptions>(arg);
				metadataInfoService = new MetadataInfoService();
				ChannelServices.RegisterChannel(new IpcServerChannel(null, options.PortName), false);
				RemotingServices.Marshal(metadataInfoService, options.ObjectName);
			}
			catch {
				return -1;
			}
			return 0;
		}
	}
}
