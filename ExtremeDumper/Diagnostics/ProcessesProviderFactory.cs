namespace ExtremeDumper.Diagnostics;

/// <summary>
/// Process infos provider factory
/// </summary>
public static class ProcessesProviderFactory {
	/// <summary>
	/// Create
	/// </summary>
	/// <returns></returns>
	public static IProcessesProvider Create() {
		return new DefaultProcessesProvider();
	}
}
