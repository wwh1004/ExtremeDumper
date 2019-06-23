using System;
using System.Text;
using System.Windows.Forms;

namespace ExtremeDumper.Forms {
	/// <summary>
	/// 全局错误捕获
	/// </summary>
	internal static class GlobalExceptionCatcher {
		/// <summary>
		/// 指示是否使用过
		/// </summary>
		private static bool _used;

		/// <summary>
		/// 自动捕获所有异常
		/// </summary>
		public static void Catch() {
			if (!_used) {
				_used = true;
				Application.ThreadException += (object sender, System.Threading.ThreadExceptionEventArgs e) => ShowDetailException(e.Exception);
				AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => ShowDetailException((Exception)e.ExceptionObject);
			}
		}

		private static void ShowDetailException(Exception ex) {
			StringBuilder message;

			message = new StringBuilder();
			message.AppendLine("Message：\n" + ex.Message);
			message.AppendLine("Source：\n" + ex.Source);
			message.AppendLine("StackTrace：\n" + ex.StackTrace);
			message.AppendLine("TargetSite：\n" + ex.TargetSite.ToString());
			MessageBoxStub.Show(message.ToString(), MessageBoxIcon.Error);
		}
	}
}
