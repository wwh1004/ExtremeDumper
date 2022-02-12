#if NETFRAMEWORK || NETSTANDARD2_0
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
sealed class NotNullWhenAttribute : Attribute {
	public bool ReturnValue { get; }

	public NotNullWhenAttribute(bool returnValue) {
		ReturnValue = returnValue;
	}
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
sealed class DoesNotReturnAttribute : Attribute {
	public DoesNotReturnAttribute() {
	}
}

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
sealed class DoesNotReturnIfAttribute : Attribute {
	public bool ParameterValue { get; }

	public DoesNotReturnIfAttribute(bool parameterValue) {
		ParameterValue = parameterValue;
	}
}
#endif
