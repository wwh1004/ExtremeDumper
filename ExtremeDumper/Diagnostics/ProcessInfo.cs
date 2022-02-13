using System;

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
	public ModuleInfo ClrModule { get; }

	public bool IsCLR2 => string.Equals(ClrModule.Name, "mscorwks.dll", StringComparison.OrdinalIgnoreCase);

	public bool IsCLR4 => string.Equals(ClrModule.Name, "clr.dll", StringComparison.OrdinalIgnoreCase);

	public bool IsCoreCLR => string.Equals(ClrModule.Name, "coreclr.dll", StringComparison.OrdinalIgnoreCase);

	public DotNetProcessInfo() {
		ClrModule = new ModuleInfo();
	}

	public DotNetProcessInfo(uint id, string name, string filePath, bool is64Bit, ModuleInfo clrModule) : base(id, name, filePath, is64Bit) {
		ClrModule = clrModule ?? new ModuleInfo();
	}
}
