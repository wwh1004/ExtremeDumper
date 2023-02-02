using System;
using dnlib.PE;

namespace ExtremeDumper.Dumping;

/// <summary>
/// 转储器的接口类
/// </summary>
public interface IDumper : IDisposable {
	/// <summary>
	/// 转储指定模块
	/// </summary>
	/// <param name="moduleHandle">模块句柄</param>
	/// <param name="imageLayout">程序集在内存中的格式</param>
	/// <param name="filePath">将转储文件保存到指定路径</param>
	/// <returns></returns>
	bool DumpModule(nuint moduleHandle, ImageLayout imageLayout, string filePath);

	/// <summary>
	/// 转储指定进程，返回转储文件数
	/// </summary>
	/// <param name="directoryPath">转储的文件保存到指定文件夹下</param>
	/// <returns></returns>
	int DumpProcess(string directoryPath);

	bool SanitizeNames { get; set; }
}
