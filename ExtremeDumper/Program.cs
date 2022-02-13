using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ExtremeDumper.AntiAntiDump;
using ExtremeDumper.Forms;
using NativeSharp;

namespace ExtremeDumper;

public static class Program {
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
			b = client.EnableMultiDomain(out var otherClients);
			Debug2.Assert(b && (!REMOTE || otherClients.Length == 1));
			b = otherClients.All(t => t.Connect(1000));
			Debug2.Assert(b);
			var aggregator = new AADClients(client, otherClients);
			b = aggregator.GetModules(out var modules);
			Debug2.Assert(b);
			foreach (var module in modules!) {
				b = aggregator.GetPEInfo(module, out var peInfo);
				if (peInfo is null || peInfo.LoadedLayout.IsInvalid)
					continue;
				// may be ngen image and corresponding IL image not loaded

				b = aggregator.GetMetadata(module, out var metadata);
				Debug2.Assert(b);

				var imageLayout = FindMetadataImageLayout(peInfo, metadata!.MetadataAddress);
				if (imageLayout is null)
					throw new InvalidOperationException("Can't find the PEImageLayout where the metadata is located");
				Console.WriteLine($"{module.Name}: image at 0x{imageLayout.ImageBase:X} metadata rva 0x{metadata.MetadataAddress - imageLayout.ImageBase:X}");
			}
			aggregator.DisconnectAll();
		}
		GlobalExceptionCatcher.Catch();
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		Application.Run(new ProcessesForm());
	}

	static PEImageLayout? FindMetadataImageLayout(PEInfo peInfo, ulong metadataAddress) {
		if (Check(peInfo.FlatLayout, metadataAddress))
			return peInfo.FlatLayout;
		if (Check(peInfo.MappedLayout, metadataAddress))
			return peInfo.MappedLayout;
		if (Check(peInfo.LoadedLayout, metadataAddress))
			return peInfo.LoadedLayout;
		return null;

		static bool Check(PEImageLayout imageLayout, ulong metadataAddress) {
			if (imageLayout.IsInvalid)
				return false;
			return imageLayout.ImageBase <= metadataAddress && metadataAddress < imageLayout.ImageBase + imageLayout.ImageSize;
		}
	}

	static void CreatServer(string pipeName, bool remote) {
		if (remote) {
			var process = NativeProcess.Open(CreateProcess("MetadataLocator.Test.CLR40.x86.cex.exe"));
			bool b = process.InjectManaged(
				AADCoreInjector.GetAADCorePath(),
				"ExtremeDumper.AntiAntiDump.Injection",
				"Main",
				pipeName,
				InjectionClrVersion.V4);
			Debug2.Assert(b);
		}
		else {
			new Thread(() => {
				var server = AADServer.Create(pipeName);
				Debug2.Assert(server is not null);
				server.Listen();
			}).Start();
		}
	}

	static uint CreateProcess(string fileName) {
		foreach (var oldProcess in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(fileName))) {
			oldProcess.Kill();
			oldProcess.Dispose();
		}
		using var newProcess = Process.Start(fileName);
		return (uint)newProcess.Id;
	}
}
