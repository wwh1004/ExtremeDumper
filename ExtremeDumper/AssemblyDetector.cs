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
