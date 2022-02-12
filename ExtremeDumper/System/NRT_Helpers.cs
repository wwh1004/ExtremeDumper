using System.Diagnostics.CodeAnalysis;

namespace System.Diagnostics;

static class Debug2 {
	[Conditional("DEBUG")]
	public static void Assert([DoesNotReturnIf(false)] bool condition) {
		Debug.Assert(condition);
	}

	[Conditional("DEBUG")]
	public static void Assert([DoesNotReturnIf(false)] bool condition, string? message) {
		Debug.Assert(condition, message);
	}
}
