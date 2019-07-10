using System;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;

namespace ExtremeDumper.AntiAntiDump {
	public static class Injection {
		public static int Main(string arg) {
			try {
				Options options;
				MetadataService metadataService;

				options = Options.Deserialize(arg);
				metadataService = new MetadataService();
				ChannelServices.RegisterChannel(new IpcServerChannel(null, options.PortName), false);
				RemotingServices.Marshal(metadataService, options.ObjectName);
			}
			catch {
				return -1;
			}
			return 0;
		}

		public sealed class Options {
			private string _portName;
			private string _objectName;

			public string PortName {
				get => _portName;
				set => _portName = value;
			}

			public string ObjectName {
				get => _objectName;
				set => _objectName = value;
			}

			public string Serialize() {
				return string.Join("|", new string[] {
					ToBase64(PortName),
					ToBase64(ObjectName)
				});
			}

			public static Options Deserialize(string data) {
				if (string.IsNullOrEmpty(data))
					throw new ArgumentNullException(nameof(data));

				string[] arguments;
				Options options;

				arguments = data.Split('|').Select(t => FromBase64(t)).ToArray();
				if (arguments.Length != 2)
					throw new ArgumentException(nameof(data));
				options = new Options {
					PortName = arguments[0],
					ObjectName = arguments[1]
				};
				return options;
			}

			private static string ToBase64(string value) {
				return Convert.ToBase64String(Encoding.Unicode.GetBytes(value));
			}

			private static string FromBase64(string value) {
				return Encoding.Unicode.GetString(Convert.FromBase64String(value));
			}
		}
	}
}
