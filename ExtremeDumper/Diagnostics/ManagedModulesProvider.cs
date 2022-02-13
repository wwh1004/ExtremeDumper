using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExtremeDumper.Dumping;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Utilities;
using NativeSharp;

namespace ExtremeDumper.Diagnostics;

sealed unsafe class ManagedModulesProvider : IModulesProvider {
	readonly uint processId;

	public ManagedModulesProvider(uint processId) {
		this.processId = processId;
	}

	public IEnumerable<ModuleInfo> EnumerateModules() {
		using var process = NativeProcess.Open(processId);
		if (process.IsInvalid)
			throw new InvalidOperationException();
		using var dataTarget = DataTarget.AttachToProcess((int)processId, 1000, AttachFlag.Passive);
		dataTarget.SymbolLocator = DummySymbolLocator.Instance;
		foreach (var runtime in dataTarget.ClrVersions.Select(t => t.CreateRuntime())) {
			var clrVersion = runtime.ClrInfo.Version.ToString();
			foreach (var domain in runtime.AppDomains) {
				var domainName = domain.Name;
				foreach (var module in domain.Modules) {
					if (module.ImageBase == 0)
						continue;
					if (!IsValidPEMagic(module, process))
						continue;
					// .NET 3.5有nlp文件，但是被认为是.NET模块

					GetModuleFileInfo(module, out var name, out var path);
					uint size = GetModuleSize(module, process);
					yield return new DotNetModuleInfo(name, (nuint)module.ImageBase, size, path, domainName, clrVersion);
				}
			}
		}
	}

	static bool IsValidPEMagic(ClrModule module, NativeProcess process) {
		byte[] buffer = new byte[2];
		if (!process.TryReadBytes((void*)module.ImageBase, buffer))
			return false;

		return buffer[0] == 'M' && buffer[1] == 'Z';
	}

	static void GetModuleFileInfo(ClrModule module, out string name, out string path) {
		if (string.IsNullOrEmpty(module.Name)) {
			// In memory and obfuscated
			name = "<<EmptyName>>";
			path = "InMemory";
			return;
		}

		if (!module.Name.Contains(",")) {
			// In disk
			name = Path.GetFileName(module.Name);
			path = module.Name;
		}
		else {
			// In memory, module.Name is reflection assembly name.
			name = module.Name.Split(',')[0];
			path = "InMemory";
		}
	}

	static uint GetModuleSize(ClrModule module, NativeProcess process) {
		uint size = (uint)module.Size;
		if (size != 0)
			return size;

		byte[] peHeader = new byte[0x1000];
		if (!process.TryReadBytes((void*)module.ImageBase, peHeader))
			return 0;

		try {
			return PEImageDumper.GetImageSize(peHeader, dnlib.PE.ImageLayout.Memory);
		}
		catch {
			return 0;
		}
	}

	sealed class DummySymbolLocator : SymbolLocator {
		public static DummySymbolLocator Instance { get; } = new();
		private DummySymbolLocator() { }
		public override string FindBinary(string fileName, int buildTimeStamp, int imageSize, bool checkProperties = true) { return string.Empty; }
		public override Task<string> FindBinaryAsync(string fileName, int buildTimeStamp, int imageSize, bool checkProperties = true) { return Task.FromResult(string.Empty); }
		public override string FindPdb(string pdbName, Guid pdbIndexGuid, int pdbIndexAge) { return string.Empty; }
		public override Task<string> FindPdbAsync(string pdbName, Guid pdbIndexGuid, int pdbIndexAge) { return Task.FromResult(string.Empty); }
		protected override Task CopyStreamToFileAsync(Stream input, string fullSrcPath, string fullDestPath, long size) { throw new NotImplementedException(); }
	}
}
