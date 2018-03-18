using System;
using System.IO;
using System.Text;
using dnlib.DotNet;
using static InjectingDumper.NativeMethods;

namespace InjectingDumper
{
    public static class Dumper
    {
        private static readonly bool Is64BitProcess = IntPtr.Size == 8;

        public static int TryDumpModule(string arg)
        {
            string[] args;
            IntPtr moduleHandle;
            uint mdRva;
            uint mdSize;
            string directory;

            arg = Encoding.Unicode.GetString(Convert.FromBase64String(arg));
            args = arg.Split('|');
            moduleHandle = (IntPtr)ulong.Parse(args[0]);
            mdRva = uint.Parse(args[1]);
            mdSize = uint.Parse(args[2]);
            directory = args[3];
            Console.WriteLine("获取参数完毕");
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
            catch (Exception ex)
            {
                //Console.WriteLine("未修复方案错误");
                //ShowDetailException(ex);
                return false;
            }
            return true;
        }

        private static unsafe bool TryDumpModuleFix(IntPtr moduleHandle, uint mdRva, uint mdSize, string directory)
        {
            MEMORY_BASIC_INFORMATION mbi;
            uint oldProtect;
            NativeMemoryIO memory;
            uint peOffset;
            uint mddRva;

            VirtualQuery(moduleHandle, out mbi, MEMORY_BASIC_INFORMATION.Size);
            VirtualProtect(moduleHandle, (uint)mbi.RegionSize, PAGE_EXECUTE_READWRITE, out oldProtect);
#pragma warning disable IDE0017
            memory = new NativeMemoryIO(moduleHandle);
#pragma warning restore IDE0017
            memory.Position = 0x3C;
            peOffset = memory.ReadUInt32();
            memory.Position = peOffset + (Is64BitProcess ? 0xF8 : 0xE8);
            mddRva = (uint)memory.Position + 4;
            memory.WriteUInt32(mddRva);
            //.Net MetaData Directory RVA
            for (int i = 0; i < 72; i += 4)
            {
                memory.Position = mddRva + i;
                switch (i)
                {
                    case 0:
                        //cb
                        memory.WriteUInt32(0x48);
                        break;
                    case 4:
                        //MajorRuntimeVersion
                        //MinorRuntimeVersion
                        memory.WriteUInt32(2);
                        break;
                    case 8:
                        //MetaData RVA
                        memory.WriteUInt32(mdRva);
                        break;
                    case 12:
                        //MetaData Size
                        memory.WriteUInt32(mdSize);
                        break;
                    case 16:
                        //Flags
                        memory.WriteUInt32(0);
                        break;
                    case 20:
                        //EntryPointTokenOrRVA
                        memory.WriteUInt32(0x6000004);
                        break;
                    default:
                        memory.WriteUInt32(0);
                        break;
                }
            }
            VirtualProtect(moduleHandle, (uint)mbi.RegionSize, oldProtect, out oldProtect);
            return TryDumpModule(moduleHandle, directory);
        }

        //private static void ShowDetailException(Exception ex)
        //{
        //    StringBuilder message;

        //    message = new StringBuilder();
        //    message.AppendLine("Message：\n" + ex.Message);
        //    message.AppendLine("Source：\n" + ex.Source);
        //    message.AppendLine("StackTrace：\n" + ex.StackTrace);
        //    message.AppendLine("TargetSite：\n" + ex.TargetSite.ToString());
        //    Console.WriteLine(message.ToString());
        //}

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
