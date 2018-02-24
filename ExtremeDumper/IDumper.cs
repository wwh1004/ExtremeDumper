using System;

namespace ExtremeDumper
{
    /// <summary>
    /// 转储器的接口类
    /// </summary>
    public interface IDumper
    {
        /// <summary>
        /// 转储指定Native模块
        /// </summary>
        /// <param name="moduleHandle">模块句柄</param>
        /// <param name="path">转储的文件保存到指定文件夹下</param>
        /// <returns></returns>
        bool DumpModule(IntPtr moduleHandle, string path);

        /// <summary>
        /// 转储指定.Net模块
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="path">转储的文件保存到指定文件夹下</param>
        /// <returns></returns>
        bool DumpModule(uint moduleId, string path);

        /// <summary>
        /// 转储指定进程，返回转储文件数
        /// </summary>
        /// <param name="path">转储的文件保存到指定文件夹下</param>
        /// <returns></returns>
        int DumpProcess(string path);
    }
}
