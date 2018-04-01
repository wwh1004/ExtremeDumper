using System;
using static ExtremeDumper.Dumper.NativeMethods;

namespace ExtremeDumper.Dumper
{
    internal static class EnvironmentCache
    {
        public static readonly bool Is64BitOperatingSystem = Environment.Is64BitOperatingSystem;

        public static readonly SYSTEM_INFO SystemInfo;

        static EnvironmentCache()
        {
            GetSystemInfo(out SystemInfo);
        }
    }
}
