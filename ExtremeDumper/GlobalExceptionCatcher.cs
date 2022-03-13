using System;
using System.Windows.Forms;
using ExtremeDumper.Logging;

namespace ExtremeDumper;

/// <summary>
/// 全局错误捕获
/// </summary>
static class GlobalExceptionCatcher {
	static bool isStarted;

	/// <summary>
	/// 自动捕获所有异常
	/// </summary>
	public static void Catch() {
		if (!isStarted) {
			Application.ThreadException += (sender, e) => Logger.Exception(e.Exception);
			AppDomain.CurrentDomain.UnhandledException += (sender, e) => Logger.Exception(e.ExceptionObject as Exception);
			isStarted = true;
		}
	}
}
