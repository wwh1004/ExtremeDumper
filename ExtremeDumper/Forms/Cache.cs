using System;
using System.Drawing;

namespace ExtremeDumper.Forms
{
    internal static class Cache
    {
        public static readonly bool Is64BitOperatingSystem = Environment.Is64BitOperatingSystem;

        public static readonly Color DotNetColor = Color.YellowGreen;
    }
}
