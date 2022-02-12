namespace System;

static class Array2 {
	public static T[] Empty<T>() {
		return EmptyArray<T>.Value;
	}

	static class EmptyArray<T> {
#pragma warning disable CA1825
		public static readonly T[] Value = new T[0];
#pragma warning restore CA1825
	}
}
