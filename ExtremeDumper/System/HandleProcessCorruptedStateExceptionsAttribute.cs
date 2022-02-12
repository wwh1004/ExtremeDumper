#if !NET40_OR_GREATER
namespace System.Runtime.ExceptionServices;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
sealed class HandleProcessCorruptedStateExceptionsAttribute : Attribute {
}
#endif
