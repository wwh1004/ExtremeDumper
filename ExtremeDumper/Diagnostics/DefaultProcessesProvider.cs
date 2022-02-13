using System.Collections.Generic;
using System.IO;
using System.Linq;
using NativeSharp;

namespace ExtremeDumper.Diagnostics;

sealed class DefaultProcessesProvider : IProcessesProvider {
	public IEnumerable<ProcessInfo> EnumerateProcesses() {
		var processIds = NativeProcess.GetAllProcessIds();
		if (processIds is null || processIds.Length == 0)
			yield break;
		foreach (uint processId in processIds) {
			if (processId == 0)
				continue;

			var processInfo = GetProcessInfo(processId);
			if (processInfo is not null)
				yield return processInfo;
		}
	}

	static ProcessInfo? GetProcessInfo(uint processId) {
		var modulesProvider = ModulesProviderFactory.Create(processId, ModulesProviderType.Unmanaged);
		var mainModule = modulesProvider.EnumerateModules().FirstOrDefault();
		if (mainModule is null)
			return null;
		// insufficient privileges

		var name = mainModule.Name;
		var path = mainModule.FilePath;
		var clrModules = modulesProvider.EnumerateModules().Where(t => t.Name.ToUpperInvariant() is "MSCORWKS.DLL" or "CLR.DLL" or "CORECLR.DLL").ToArray();
		if (clrModules.Length != 0)
			return new DotNetProcessInfo(processId, name, path, Is64BitPE(clrModules[0].FilePath), clrModules);
		else
			return new ProcessInfo(processId, name, path, Is64BitPE(mainModule.FilePath));
	}

	static bool Is64BitPE(string filePath) {
		return Is64BitPE(filePath, out bool is64Bit) && is64Bit;
	}

	static bool Is64BitPE(string filePath, out bool is64Bit) {
		try {
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var reader = new BinaryReader(stream);
			reader.BaseStream.Position = 0x3C;
			uint peOffset = reader.ReadUInt32();
			reader.BaseStream.Position = peOffset + 0x18;
			ushort magic = reader.ReadUInt16();
			if (magic != 0x010B && magic != 0x020B)
				throw new InvalidDataException();
			is64Bit = magic == 0x020B;
			return true;
		}
		catch {
			is64Bit = false;
			return false;
		}
	}
}
