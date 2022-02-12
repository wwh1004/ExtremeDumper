using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using dnlib.DotNet;
using ExtremeDumper.AntiAntiDump;
using ExtremeDumper.Forms;
using NativeSharp;

namespace ExtremeDumper;

public static class Program {
	static byte[] GetAntiAntiDumpDll() {
		using var module = ModuleDefMD.Load(typeof(AADServer).Module);
		using var stream = new MemoryStream();
		module.Write(stream);
		return stream.ToArray();
	}

	[STAThread]
	public static void Main() {
		if (false) {
			const bool REMOTE = true;

			var pipeName = "wwhtest" + Environment.TickCount;
			CreatServer(pipeName, REMOTE);
			using var client = AADClient.Create(pipeName);
			Debug2.Assert(client is not null);
			bool b = client.Connect(1000);
			Debug2.Assert(b);
			b = client.EnableMultiDomain(1000, out var otherClients);
			Debug2.Assert(b && (!REMOTE || otherClients.Length == 1));
			var aggregator = new AADClientAggregator(client, otherClients);
			b = aggregator.GetModules(out var modules);
			Debug2.Assert(b);
			foreach (var module in modules) {
				b = aggregator.GetMetadata(module, out var md);
				if (b)
					Console.WriteLine($"{module.Name}: 0x{md.MetadataRVA:X4}");
				else
					Console.WriteLine($"{module.Name}: null");
			}
			aggregator.DisconnectAll();
		}
		GlobalExceptionCatcher.Catch();
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		Application.Run(new ProcessesForm());
	}

	static void CreatServer(string pipeName, bool remote) {
		if (remote) {
			var pids = NativeProcess.GetProcessIdsByName("MetadataLocator.Test.CLR40.x86.cex.exe").ToArray();
			if (pids.Length != 1)
				throw new InvalidOperationException();
			var process = NativeProcess.Open(pids[0]);
			process.InjectManaged(
				Path.GetFullPath("ExtremeDumper.AntiAntiDump.dll"),
				"ExtremeDumper.AntiAntiDump.Injection",
				"Main",
				pipeName,
				InjectionClrVersion.V4);
		}
		else {
			new Thread(() => {
				var server = AADServer.Create(pipeName);
				server.Listen();
			}).Start();
		}
	}
}
