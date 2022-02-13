using System.Collections.Generic;

namespace ExtremeDumper.Diagnostics;

public interface IProcessesProvider {
	IEnumerable<ProcessInfo> EnumerateProcesses();
}
