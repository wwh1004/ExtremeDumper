using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using Tool.Logging;

namespace ExtremeDumper.Logging;

static partial class Logger {
	enum StackCrawlMark {
		LookForMyCaller = 1
		// CLR内部实现似乎没有处理LookForMyCallersCaller，枚举值LookForMyCallersCaller无效
	}

	delegate MethodBase InternalGetCurrentMethodDelegate(ref StackCrawlMark stackMark);

	static readonly InternalGetCurrentMethodDelegate internalGetCurrentMethod = CreateInternalGetCurrentMethod();
	static readonly Dictionary<Type, string> typeAliases = GetTypeAliases();

	public static readonly bool CallerNameWithType = true;

#if DEBUG
	public const bool BreakOnError = true;
#endif

	public static LogLevel Level { get => Tool.Logging.Logger.Level; set => Tool.Logging.Logger.Level = value; }

	public static bool IsAsync { get => Tool.Logging.Logger.IsAsync; set => Tool.Logging.Logger.IsAsync = value; }

	public static int Indent { get; set; }

	public static void Info() {
		Tool.Logging.Logger.Info();
	}

	public static void Info(string? value, [CallerMemberName] string callerName = "") {
		string prefix;
		if (CallerNameWithType) {
			var stackMark = StackCrawlMark.LookForMyCaller;
			var caller = internalGetCurrentMethod(ref stackMark);
			prefix = GetPrefix(callerName, caller);
		}
		else {
			prefix = GetPrefix(callerName);
		}
		Tool.Logging.Logger.Info("[Info]      | " + prefix + value);
	}

	public static void Warning(string? value, [CallerMemberName] string callerName = "") {
		string prefix;
		if (CallerNameWithType) {
			var stackMark = StackCrawlMark.LookForMyCaller;
			var caller = internalGetCurrentMethod(ref stackMark);
			prefix = GetPrefix(callerName, caller);
		}
		else {
			prefix = GetPrefix(callerName);
		}
		Tool.Logging.Logger.Warning("[Warning]   | " + prefix + value);
	}

	public static void Error(string? value, [CallerMemberName] string callerName = "") {
		string prefix;
		if (CallerNameWithType) {
			var stackMark = StackCrawlMark.LookForMyCaller;
			var caller = internalGetCurrentMethod(ref stackMark);
			prefix = GetPrefix(callerName, caller);
		}
		else {
			prefix = GetPrefix(callerName);
		}
		Tool.Logging.Logger.Error("[Error]     | " + prefix + value);
#if DEBUG
		if (BreakOnError) {
			Flush();
			System.Diagnostics.Debug2.Assert(false);
		}
#endif
	}

	public static void Verbose1(string? value, [CallerMemberName] string callerName = "") {
		if (Level < LogLevel.Verbose1)
			return;
		string prefix;
		if (CallerNameWithType) {
			var stackMark = StackCrawlMark.LookForMyCaller;
			var caller = internalGetCurrentMethod(ref stackMark);
			prefix = GetPrefix(callerName, caller);
		}
		else {
			prefix = GetPrefix(callerName);
		}
		Tool.Logging.Logger.Verbose1("[Verbose1]  | " + prefix + value);
	}

	public static void Verbose1(ref Verbose1InterpolatedStringHandler value, [CallerMemberName] string callerName = "") {
		string? value2 = value.ToStringAndClear();
		if (value2 is null)
			return;
		string prefix;
		if (CallerNameWithType) {
			var stackMark = StackCrawlMark.LookForMyCaller;
			var caller = internalGetCurrentMethod(ref stackMark);
			prefix = GetPrefix(callerName, caller);
		}
		else {
			prefix = GetPrefix(callerName);
		}
		Tool.Logging.Logger.Verbose1("[Verbose1]  | " + prefix + value2);
	}

	public static void Verbose2(string? value, [CallerMemberName] string callerName = "") {
		if (Level < LogLevel.Verbose2)
			return;
		string prefix;
		if (CallerNameWithType) {
			var stackMark = StackCrawlMark.LookForMyCaller;
			var caller = internalGetCurrentMethod(ref stackMark);
			prefix = GetPrefix(callerName, caller);
		}
		else {
			prefix = GetPrefix(callerName);
		}
		Tool.Logging.Logger.Verbose2("[Verbose2]  | " + prefix + value);
	}

	public static void Verbose2(ref Verbose2InterpolatedStringHandler value, [CallerMemberName] string callerName = "") {
		string? value2 = value.ToStringAndClear();
		if (value2 is null)
			return;
		string prefix;
		if (CallerNameWithType) {
			var stackMark = StackCrawlMark.LookForMyCaller;
			var caller = internalGetCurrentMethod(ref stackMark);
			prefix = GetPrefix(callerName, caller);
		}
		else {
			prefix = GetPrefix(callerName);
		}
		Tool.Logging.Logger.Verbose2("[Verbose2]  | " + prefix + value2);
	}

	public static void Verbose3(string? value, [CallerMemberName] string callerName = "") {
		if (Level < LogLevel.Verbose3)
			return;
		string prefix;
		if (CallerNameWithType) {
			var stackMark = StackCrawlMark.LookForMyCaller;
			var caller = internalGetCurrentMethod(ref stackMark);
			prefix = GetPrefix(callerName, caller);
		}
		else {
			prefix = GetPrefix(callerName);
		}
		Tool.Logging.Logger.Verbose3("[Verbose3]  | " + prefix + value);
	}

	public static void Verbose3(ref Verbose3InterpolatedStringHandler value, [CallerMemberName] string callerName = "") {
		string? value2 = value.ToStringAndClear();
		if (value2 is null)
			return;
		string prefix;
		if (CallerNameWithType) {
			var stackMark = StackCrawlMark.LookForMyCaller;
			var caller = internalGetCurrentMethod(ref stackMark);
			prefix = GetPrefix(callerName, caller);
		}
		else {
			prefix = GetPrefix(callerName);
		}
		Tool.Logging.Logger.Verbose3("[Verbose3]  | " + prefix + value2);
	}

	public static void Exception(Exception? value, [CallerMemberName] string callerName = "") {
		string prefix;
		if (CallerNameWithType) {
			var stackMark = StackCrawlMark.LookForMyCaller;
			var caller = internalGetCurrentMethod(ref stackMark);
			prefix = GetPrefix(callerName, caller);
		}
		else {
			prefix = GetPrefix(callerName);
		}
		Tool.Logging.Logger.Error("[Exception] | " + prefix + FormatException(value));
#if DEBUG
		if (BreakOnError) {
			Flush();
			System.Diagnostics.Debug2.Assert(false);
		}
#endif
	}

	public static void Flush() {
		Tool.Logging.Logger.Flush();
	}

	static string GetPrefix(string callerName, MethodBase caller) {
		var type = GetUserType(caller.DeclaringType);
		if (type is not null) {
			var typeName = typeAliases.TryGetValue(type, out var typeAlias) ? typeAlias : type.Name;
			callerName = typeName + "." + callerName;
		}
		return GetPrefix(callerName.ToString());
	}

	static Type? GetUserType(Type? type) {
		if (type is null)
			return null;
		return type.Name.StartsWith("<", StringComparison.Ordinal) ? GetUserType(type.DeclaringType) : type;
	}

	static string GetPrefix(string callerName) {
		return $"{new string(' ', Indent * 2)}[{callerName}] ";
	}

	static string FormatException(Exception? exception) {
		var sb = new StringBuilder();
		DumpException(exception, sb);
		return sb.ToString();
	}

	static void DumpException(Exception? exception, StringBuilder sb) {
		exception ??= new ArgumentNullException(nameof(exception), "<No exception object>");
		sb.AppendLine($"Type: {Environment.NewLine}{exception.GetType().FullName}");
		sb.AppendLine($"Message: {Environment.NewLine}{exception.Message}");
		sb.AppendLine($"Source: {Environment.NewLine}{exception.Source}");
		sb.AppendLine($"StackTrace: {Environment.NewLine}{exception.StackTrace}");
		sb.AppendLine($"TargetSite: {Environment.NewLine}{exception.TargetSite}");
		sb.AppendLine("----------------------------------------");
		if (exception.InnerException is not null)
			DumpException(exception.InnerException, sb);
		if (exception is ReflectionTypeLoadException reflectionTypeLoadException) {
			foreach (var loaderException in reflectionTypeLoadException.LoaderExceptions)
				DumpException(loaderException, sb);
		}
	}

	static InternalGetCurrentMethodDelegate CreateInternalGetCurrentMethod() {
		var dynamicMethod = new DynamicMethod("InternalGetCurrentMethod_Proxy", typeof(MethodBase), new[] { typeof(StackCrawlMark).MakeByRefType() }, typeof(Logger), true);
		var generator = dynamicMethod.GetILGenerator();
		var internalGetCurrentMethod = typeof(MethodBase).Module.GetType("System.Reflection.RuntimeMethodInfo").GetMethod("InternalGetCurrentMethod", BindingFlags.NonPublic | BindingFlags.Static);

		generator.Emit(OpCodes.Ldarg_0);
		generator.Emit(OpCodes.Call, internalGetCurrentMethod);
		generator.Emit(OpCodes.Ret);

		return (InternalGetCurrentMethodDelegate)dynamicMethod.CreateDelegate(typeof(InternalGetCurrentMethodDelegate));
	}

	static Dictionary<Type, string> GetTypeAliases() {
		var aliases = new Dictionary<Type, string>();
		foreach (var type in typeof(Logger).Module.GetTypes()) {
			var attribute = type.GetCustomAttribute<TypeLoggingAliasAttribute>();
			if (attribute is not null)
				aliases.Add(type, attribute.Alias);
		}
		return aliases;
	}
}
