namespace ExtremeDumper.Injecting;

/// <summary>
/// 注入时使用的CLR版本
/// </summary>
enum InjectionClrVersion {
	///// <summary>
	///// 自动选择，由要注入的程序集本身决定
	///// </summary>
	//Auto,

	/// <summary>
	/// v2.0.50727
	/// </summary>
	V2 = 1,

	/// <summary>
	/// v4.0.30319
	/// </summary>
	V4
}

/// <summary>
/// Assembly and dll injector
/// </summary>
static class Injector {
	/// <summary>
	/// 注入托管DLL
	/// </summary>
	/// <param name="processId"></param>
	/// <param name="assemblyPath">要注入程序集的路径</param>
	/// <param name="typeName">类型名（命名空间+类型名，比如NamespaceA.ClassB）</param>
	/// <param name="methodName">方法名（比如MethodC），该方法必须具有此类签名static int MethodName(string)，比如private static int InjectingMain(string argument)</param>
	/// <param name="argument">参数，可传入 <see langword="null"/></param>
	/// <param name="clrVersion">使用的CLR版本</param>
	/// <returns></returns>
	public static bool InjectManaged(uint processId, string assemblyPath, string typeName, string methodName, string argument, InjectionClrVersion clrVersion) {
		using var process = NativeSharp.NativeProcess.Open(processId);
		if (process.IsInvalid)
			return false;
		return process.InjectManaged(assemblyPath, typeName, methodName, argument, (NativeSharp.InjectionClrVersion)clrVersion);
	}

	/// <summary>
	/// 注入托管DLL，并获取被调用方法的返回值（警告：被调用方法返回后才能获取到返回值，<see cref="InjectManaged(string, string, string, string, out int)"/>方法将一直等待到被调用方法返回。如果仅注入程序集而不需要获取返回值，请使用重载版本<see cref="InjectManaged(string, string, string, string)"/>）
	/// </summary>
	/// <param name="processId"></param>
	/// <param name="assemblyPath">要注入程序集的路径</param>
	/// <param name="typeName">类型名（命名空间+类型名，比如NamespaceA.ClassB）</param>
	/// <param name="methodName">方法名（比如MethodC），该方法必须具有此类签名static int MethodName(string)，比如private static int InjectingMain(string argument)</param>
	/// <param name="argument">参数，可传入 <see langword="null"/></param>
	/// <param name="clrVersion">使用的CLR版本</param>
	/// <param name="returnValue">被调用方法返回的整数值</param>
	/// <returns></returns>
	public static bool InjectManagedAndWait(uint processId, string assemblyPath, string typeName, string methodName, string argument, InjectionClrVersion clrVersion, out int returnValue) {
		returnValue = 0;
		using var process = NativeSharp.NativeProcess.Open(processId);
		if (process.IsInvalid)
			return false;
		return process.InjectManaged(assemblyPath, typeName, methodName, argument, (NativeSharp.InjectionClrVersion)clrVersion, out returnValue);
	}

	/// <summary>
	/// 注入非托管DLL
	/// </summary>
	/// <param name="processId"></param>
	/// <param name="dllPath">要注入DLL的路径</param>
	/// <returns></returns>
	public static bool InjectUnmanaged(uint processId, string dllPath) {
		using var process = NativeSharp.NativeProcess.Open(processId);
		if (process.IsInvalid)
			return false;
		return process.InjectUnmanaged(dllPath);
	}
}
