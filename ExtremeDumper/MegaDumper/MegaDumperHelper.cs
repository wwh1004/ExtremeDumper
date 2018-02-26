using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ExtremeDumper.MegaDumper
{
    internal static class MegaDumperHelper
    {
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        public static void CreateDirectories(string DirectoryName)
        {
            if (!Directory.Exists(Path.Combine(DirectoryName, ".Net Assemblies")))
                Directory.CreateDirectory(Path.Combine(DirectoryName, ".Net Assemblies"));
            if (!Directory.Exists(Path.Combine(DirectoryName, "Native Dlls")))
                Directory.CreateDirectory(Path.Combine(DirectoryName, "Native Dlls"));
            if (!Directory.Exists(Path.Combine(DirectoryName, "RawDumps")))
                Directory.CreateDirectory(Path.Combine(DirectoryName, "RawDumps"));
            if (!Directory.Exists(Path.Combine(DirectoryName, "VDumps")))
                Directory.CreateDirectory(Path.Combine(DirectoryName, "VDumps"));
            if (!Directory.Exists(Path.Combine(DirectoryName, "Unknowns")))
                Directory.CreateDirectory(Path.Combine(DirectoryName, "Unknowns"));
        }

        public static void Classify(string DirectoryName)
        {
            foreach (FileInfo fileInfo in new DirectoryInfo(DirectoryName).GetFiles())
            {
                if (fileInfo.Name.StartsWith("rawdump_"))
                {
                    File.Move(fileInfo.FullName, Path.Combine(fileInfo.DirectoryName, "RawDumps", fileInfo.Name));
                    continue;
                }
                if (fileInfo.Name.StartsWith("vdump_"))
                {
                    File.Move(fileInfo.FullName, Path.Combine(fileInfo.DirectoryName, "VDumps", fileInfo.Name));
                    continue;
                }
                if (AssemblyDetector.IsAssembly(fileInfo.FullName))
                {
                    File.Move(fileInfo.FullName, Path.Combine(fileInfo.DirectoryName, ".Net Assemblies", fileInfo.Name));
                    continue;
                }
                if (fileInfo.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    File.Move(fileInfo.FullName, Path.Combine(fileInfo.DirectoryName, "Native Dlls", fileInfo.Name));
                    continue;
                }
                File.Move(fileInfo.FullName, Path.Combine(fileInfo.DirectoryName, "Unknowns", fileInfo.Name));
            }
        }

        public static string EnsureValidFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            StringBuilder newFileName;

            newFileName = new StringBuilder(fileName.Length);
            foreach (char chr in fileName)
                if (!InvalidFileNameChars.Contains(chr))
                    newFileName.Append(chr);
            return newFileName.ToString();
        }
    }
}
