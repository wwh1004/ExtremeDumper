namespace System
{
    internal static class ArrayAddIn
    {
        public static T[] Empty<T>()
        {
            return EmptyArrayAddIn<T>.Value;
        }
    }
}
