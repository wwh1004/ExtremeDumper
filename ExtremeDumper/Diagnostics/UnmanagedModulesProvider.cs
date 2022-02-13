using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ExtremeDumper.Diagnostics;

sealed class UnmanagedModulesProvider : IModulesProvider {
	readonly uint processId;

	public UnmanagedModulesProvider(uint processId) {
		this.processId = processId;
	}

	public IEnumerable<ModuleInfo> EnumerateModules() {
		var snapshotHandle = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, processId);
		if (snapshotHandle == INVALID_HANDLE_VALUE)
			yield break;

		try {
			var moduleEntry = new MODULEENTRY32 { dwSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32)) };
			if (!Module32First(snapshotHandle, ref moduleEntry))
				yield break;

			do {
				yield return new ModuleInfo(moduleEntry.szModule, moduleEntry.modBaseAddr, moduleEntry.modBaseSize, moduleEntry.szExePath);
			} while (Module32Next(snapshotHandle, ref moduleEntry));
		}
		finally {
			CloseHandle(snapshotHandle);
		}
	}

	#region NativeMethods
	const uint TH32CS_SNAPMODULE = 0x00000008;
	const uint TH32CS_SNAPMODULE32 = 0x00000010;
	static readonly nuint INVALID_HANDLE_VALUE = unchecked((nuint)(-1));

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	struct MODULEENTRY32 {
		public uint dwSize;
		public uint th32ModuleID;
		public uint th32ProcessID;
		public uint GlblcntUsage;
		public uint ProccntUsage;
		public nuint modBaseAddr;
		public uint modBaseSize;
		public nuint hModule;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string szModule;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szExePath;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	static extern bool CloseHandle(nuint hObject);

	[DllImport("kernel32.dll", SetLastError = true)]
	static extern nuint CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

	[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
	static extern bool Module32First(nuint hSnapshot, ref MODULEENTRY32 lpme);

	[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
	static extern bool Module32Next(nuint hSnapshot, ref MODULEENTRY32 lppe);
	#endregion
}
