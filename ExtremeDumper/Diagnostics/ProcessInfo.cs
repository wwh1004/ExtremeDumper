using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtremeDumper.Diagnostics;

public class ProcessInfo {
	public uint Id { get; }

	public string Name { get; }

	public string FilePath { get; }

	public bool Is64Bit { get; }

	public ProcessInfo() {
		Name = string.Empty;
		FilePath = string.Empty;
	}

	public ProcessInfo(uint id, string name, string filePath, bool is64Bit) {
		Id = id;
		Name = name ?? string.Empty;
		FilePath = filePath ?? string.Empty;
		Is64Bit = is64Bit;
	}
}

public sealed class DotNetProcessInfo : ProcessInfo {
	public IReadOnlyList<ModuleInfo> CLRModules { get; }

	public bool HasCLR2 => CLRModules.Any(t => string.Equals(t.Name, "mscorwks.dll", StringComparison.OrdinalIgnoreCase));

	public bool HasCLR4 => CLRModules.Any(t => string.Equals(t.Name, "clr.dll", StringComparison.OrdinalIgnoreCase));

	public bool HasCoreCLR => CLRModules.Any(t => string.Equals(t.Name, "coreclr.dll", StringComparison.OrdinalIgnoreCase));

	public DotNetProcessInfo() {
		CLRModules = Array2.Empty<ModuleInfo>();
	}

	public DotNetProcessInfo(uint id, string name, string filePath, bool is64Bit, IEnumerable<ModuleInfo> clrModules) : base(id, name, filePath, is64Bit) {
		CLRModules = new List<ModuleInfo>(clrModules);
	}
}
