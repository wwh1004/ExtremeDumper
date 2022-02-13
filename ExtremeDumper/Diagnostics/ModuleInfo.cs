namespace ExtremeDumper.Diagnostics;

public class ModuleInfo {
	public string Name { get; }

	public nuint ImageBase { get; }

	public uint ImageSize { get; }

	public string FilePath { get; }

	public ModuleInfo() {
		Name = string.Empty;
		FilePath = string.Empty;
	}

	public ModuleInfo(string name, nuint imageBase, uint imageSize, string filePath) {
		Name = name ?? string.Empty;
		ImageBase = imageBase;
		ImageSize = imageSize;
		FilePath = filePath ?? string.Empty;
	}
}

public sealed class DotNetModuleInfo : ModuleInfo {
	public string DomainName { get; }

	public string CLRVersion { get; }

	public DotNetModuleInfo() {
		DomainName = string.Empty;
		CLRVersion = string.Empty;
	}

	public DotNetModuleInfo(string name, nuint imageBase, uint imageSize, string filePath, string domainName, string clrVersion) : base(name, imageBase, imageSize, filePath) {
		DomainName = domainName ?? string.Empty;
		CLRVersion = clrVersion ?? string.Empty;
	}
}
