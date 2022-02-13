using System;
using System.IO;
using dnlib.DotNet;
using ExtremeDumper.Injecting;

namespace ExtremeDumper.AntiAntiDump;

static class AADCoreInjector {
	static string dllPath = string.Empty;

	public static string GetAADCorePath() {
		if (!string.IsNullOrEmpty(dllPath))
			return dllPath;

		var data = GetAADCore(true, out var name);
		dllPath = Path.Combine(Path.GetTempPath(), name);
		File.WriteAllBytes(dllPath, data);
		return dllPath;
	}

	static byte[] GetAADCore(bool obfuscate, out string fileName) {
		using var module = ModuleDefMD.Load(typeof(AADServer).Module);
		if (obfuscate) {
			var t = Guid.NewGuid().ToString();
			module.Name = $"{t}.dll";
			module.Assembly.Name = t;
		}
		fileName = $"{module.Assembly.Name}.dll";
		using var stream = new MemoryStream();
		module.Write(stream);
		return stream.ToArray();
	}

	public static AADClient Inject(uint processId, InjectionClrVersion clrVersion) {
		return Inject(processId, clrVersion, Guid.NewGuid().ToString());
	}

	public static AADClient Inject(uint processId, InjectionClrVersion clrVersion, string pipeName) {
		if (processId == 0)
			throw new ArgumentNullException(nameof(processId));
		if (string.IsNullOrEmpty(pipeName))
			throw new ArgumentException($"'{nameof(pipeName)}' cannot be null or empty.", nameof(pipeName));

		bool b = Injector.InjectManaged(processId, GetAADCorePath(), "ExtremeDumper.AntiAntiDump.Injection", "Main", pipeName, clrVersion);
		if (!b)
			throw new InvalidOperationException("Can't inject ExtremeDumper.AntiAntiDump.dll to target process.");

		var client = AADClient.Create(pipeName);
		if (client is null)
			throw new InvalidOperationException($"Can't create {nameof(AADClient)}.");

		return client;
	}
}
