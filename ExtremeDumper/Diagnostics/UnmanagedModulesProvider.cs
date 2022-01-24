using System.Collections.Generic;
using static ExtremeDumper.Diagnostics.NativeMethods;

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
			var moduleEntry = MODULEENTRY32.Default;
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
}
