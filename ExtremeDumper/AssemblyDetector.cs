using System.IO;
using dnlib.DotNet;

namespace ExtremeDumper
{
    internal static class AssemblyDetector
    {
        /// <summary>
        /// 判断是否为程序集
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static bool IsAssembly(string path)
        {
            string extension;

            extension = Path.GetExtension(path).ToUpperInvariant();
            if (extension == ".MUI" || extension == string.Empty)
                return false;
            try
            {
                ModuleDefMD.Load(path);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
