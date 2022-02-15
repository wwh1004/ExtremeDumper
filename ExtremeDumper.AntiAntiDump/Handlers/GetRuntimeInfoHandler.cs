using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ExtremeDumper.AntiAntiDump.Handlers;

sealed class GetRuntimeInfoHandler : ICommandHandler {
	public AADCommand Command => AADCommand.GetRuntimeInfo;

	public Type ParametersType => typeof(EmptySerializable);

	public Type ResultType => typeof(RuntimeInfo);

	public bool Execute(ISerializable parameters, [NotNullWhen(true)] out ISerializable? result) {
		var flavor = GetRuntimeFlavor();
		var fileName = GetRuntimeFileName(flavor);
		var fileVersion = GetFileVersion(fileName);
		result = new RuntimeInfo {
			Flavor = flavor,
			FileName = fileName,
			FileVersion = fileVersion
		};
		return true;
	}

	static RuntimeFlavor GetRuntimeFlavor() {
		var assemblyProductAttribute = (AssemblyProductAttribute)typeof(object).Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];
		string product = assemblyProductAttribute.Product;
		if (product.EndsWith("Framework", StringComparison.Ordinal)) return RuntimeFlavor.Framework;
		else if (product.EndsWith("Core", StringComparison.Ordinal)) return RuntimeFlavor.Core;
		else if (product.EndsWith("NET", StringComparison.Ordinal)) return RuntimeFlavor.Net;
		else throw new NotSupportedException();
	}

	static string GetRuntimeFileName(RuntimeFlavor flavor) {
		switch (flavor) {
		case RuntimeFlavor.Framework:
			return Environment.Version.Major == 4 ? "clr.dll" : "mscorwks.dll";
		case RuntimeFlavor.Core:
		case RuntimeFlavor.Net:
			return "coreclr.dll";
		default:
			throw new NotSupportedException();
		}
	}

	static Version GetFileVersion(string fileName) {
		var path = new StringBuilder(MAX_PATH);
		if (!GetModuleFileName(GetModuleHandle(fileName), path, MAX_PATH))
			return new Version();
		var versionInfo = FileVersionInfo.GetVersionInfo(path.ToString());
		return new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart);
	}

	#region NativeMethods
	const ushort MAX_PATH = 260;

	[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
	static extern nuint GetModuleHandle(string? lpModuleName);

	[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	static extern bool GetModuleFileName(nuint hModule, StringBuilder lpFilename, uint nSize);
	#endregion
}
