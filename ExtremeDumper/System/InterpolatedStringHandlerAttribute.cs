#if !NET6_0_OR_GREATER
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
sealed class InterpolatedStringHandlerAttribute : Attribute {
	public InterpolatedStringHandlerAttribute() {
	}
}
#endif
