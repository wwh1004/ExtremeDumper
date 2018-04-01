using System;
using System.IO;
using System.Text;

namespace ExtremeDumper.Dumper
{
    internal static class AssemblyDetector
    {
        private struct Section
        {
            public uint VirtualSize;

            public uint VirtualAddress;

            public uint SizeOfRawData;

            public uint PointerToRawData;

            public Section(uint virtualSize, uint virtualAddress, uint sizeOfRawData, uint pointerToRawData)
            {
                VirtualSize = virtualSize;
                VirtualAddress = virtualAddress;
                SizeOfRawData = sizeOfRawData;
                PointerToRawData = pointerToRawData;
            }
        }

        /// <summary>
        /// 判断是否为程序集
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static bool IsAssembly(string path)
        {
            try
            {
                BinaryReader binaryReader;
                string clrVersion;

                using (binaryReader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
                    clrVersion = GetVersionString(binaryReader);
                return !string.IsNullOrEmpty(clrVersion);
            }
            catch
            {
                return false;
            }
        }

        private static string GetVersionString(BinaryReader binaryReader)
        {
            uint peOffset;
            bool is64;
            Section[] sections;
            uint rva;
            Section? section;

            GetPEInfo(binaryReader, out peOffset, out is64);
            binaryReader.BaseStream.Position = peOffset + (is64 ? 0xF8 : 0xE8);
            rva = binaryReader.ReadUInt32();
            if (rva == 0)
                return null;
            sections = GetSections(binaryReader);
            section = GetSection(rva, sections);
            if (section == null)
                return null;
            binaryReader.BaseStream.Position = section.Value.PointerToRawData + rva - section.Value.VirtualAddress + 0x8;
            rva = binaryReader.ReadUInt32();
            if (rva == 0)
                return null;
            section = GetSection(rva, sections);
            if (section == null)
                return null;
            binaryReader.BaseStream.Position = section.Value.PointerToRawData + rva - section.Value.VirtualAddress + 0xC;
            return Encoding.UTF8.GetString(binaryReader.ReadBytes(binaryReader.ReadInt32() - 2));
        }

        private static void GetPEInfo(BinaryReader binaryReader, out uint peOffset, out bool is64)
        {
            ushort machine;

            binaryReader.BaseStream.Position = 0x3C;
            peOffset = binaryReader.ReadUInt32();
            binaryReader.BaseStream.Position = peOffset + 0x4;
            machine = binaryReader.ReadUInt16();
            is64 = machine == 0x8664;
        }

        private static Section[] GetSections(BinaryReader binaryReader)
        {
            uint ntHeaderOffset;
            bool is64;
            ushort numberOfSections;
            Section[] sections;

            GetPEInfo(binaryReader, out ntHeaderOffset, out is64);
            numberOfSections = binaryReader.ReadUInt16();
            binaryReader.BaseStream.Position = ntHeaderOffset + (is64 ? 0x108 : 0xF8);
            sections = new Section[numberOfSections];
            for (int i = 0; i < numberOfSections; i++)
            {
                binaryReader.BaseStream.Position += 0x8;
                sections[i] = new Section(binaryReader.ReadUInt32(), binaryReader.ReadUInt32(), binaryReader.ReadUInt32(), binaryReader.ReadUInt32());
                binaryReader.BaseStream.Position += 0x10;
            }
            return sections;
        }

        private static Section? GetSection(uint rva, Section[] sections)
        {
            foreach (Section section in sections)
                if (rva >= section.VirtualAddress && rva < section.VirtualAddress + Math.Max(section.VirtualSize, section.SizeOfRawData))
                    return section;
            return null;
        }
    }
}
