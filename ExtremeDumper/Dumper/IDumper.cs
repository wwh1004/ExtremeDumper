using System;

namespace ExtremeDumper.Dumper {
	/// <summary>
	/// 转储器的接口类
	/// </summary>
	public interface IDumper : IDisposable {
		/// <summary>
		/// 转储指定Native模块
		/// </summary>
		/// <param name="moduleHandle">模块句柄</param>
		/// <param name="filePath">将转储文件保存到指定路径</param>
		/// <returns></returns>
		bool DumpModule(IntPtr moduleHandle, string filePath);

		/// <summary>
		/// 转储指定进程，返回转储文件数
		/// </summary>
		/// <param name="directoryPath">转储的文件保存到指定文件夹下</param>
		/// <returns></returns>
		int DumpProcess(string directoryPath);
	}
}
