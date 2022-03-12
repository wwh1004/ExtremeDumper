using System;

namespace ExtremeDumper.Logging;

/// <summary>
/// 记录日志时，方法所在类型的别名
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
sealed class TypeLoggingAliasAttribute : Attribute {
	/// <summary>
	/// 别名
	/// </summary>
	public string Alias { get; }

	/// <summary>
	/// 构造器
	/// </summary>
	/// <param name="alias"></param>
	public TypeLoggingAliasAttribute(string alias) {
		Alias = alias ?? string.Empty;
	}
}
