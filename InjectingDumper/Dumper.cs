using System;
using System.IO;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.PE;

namespace InjectingDumper
{
    public static class Dumper
    {
        public static int TryDumpModule(string arg)
        {
            string[] args;
            IntPtr moduleHandle;
            RVA mdRva;
            uint mdSize;
            string directory;

            args = arg.Split('|');
            moduleHandle = (IntPtr)ulong.Parse(args[0]);
            mdRva = (RVA)uint.Parse(args[1]);
            mdSize = uint.Parse(args[2]);
            directory = args[3];
            return (TryDumpModule(moduleHandle, directory) || TryDumpModuleFix(moduleHandle, mdRva, mdSize, directory)) ? 1 : 0;
        }

        private static bool TryDumpModule(IntPtr moduleHandle, string directory)
        {
            ModuleDefMD moduleDefMD;
            string moduleName;

            try
            {
                moduleDefMD = ModuleDefMD.Load(moduleHandle);
                moduleName = moduleDefMD.Name.ToString();
                if (string.IsNullOrEmpty(moduleName))
                    moduleName = "<<EmptyName>>.dll";
                moduleDefMD.Write(TryRename(Path.Combine(directory, moduleName)));
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static unsafe bool TryDumpModuleFix(IntPtr moduleHandle, RVA mdRva, uint mdSize, string directory)
        {
            ModuleDefMD moduleDefMD;
            string moduleName;

            try
            {
                moduleDefMD = ModuleDefMD.Load(MetaDataCreator.Create(new PEImage(moduleHandle), true, mdRva, mdSize), null);
                moduleName = moduleDefMD.Name.ToString();
                if (string.IsNullOrEmpty(moduleName))
                    moduleName = "<<EmptyName>>.dll";
                moduleDefMD.Write(TryRename(Path.Combine(directory, moduleName)));
            }
            catch
            {
                return false;
            }
            return true;
        }

        private static string TryRename(string filePath)
        {
            if (!File.Exists(filePath))
                return filePath;

            StringBuilder filePathBuilder;
            int suffixIndex;
            string newFilePath;

            filePathBuilder = new StringBuilder(filePath);
            suffixIndex = filePath.LastIndexOf('.');
            if (suffixIndex == -1)
                suffixIndex = filePath.Length - 1;
            filePathBuilder.Insert(suffixIndex, " ( )");
            suffixIndex += 2;
            for (int i = 2; i < int.MaxValue; i++)
            {
                filePathBuilder[suffixIndex] = i.ToString()[0];
                newFilePath = filePathBuilder.ToString();
                if (!File.Exists(newFilePath))
                    return newFilePath;
            }
            throw new InvalidOperationException();
        }
    }
}
