using System.Collections.Generic;

namespace ExtremeDumper.Diagnostics;

public interface IModulesProvider {
	IEnumerable<ModuleInfo> EnumerateModules();
}
