using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ExtremeDumper.Forms
{
    /// <summary>
    /// 全局错误捕获
    /// </summary>
    internal static class GlobalExceptionCatcher
    {
        [DllImport("user32.dll", BestFitMapping = false, CharSet = CharSet.Unicode, EntryPoint = "MessageBoxW", ExactSpelling = true, SetLastError = true)]
        private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

        /// <summary>
        /// 指示是否使用过
        /// </summary>
        private static bool _used;

        /// <summary>
        /// 自动捕获所有异常
        /// </summary>
        public static void Catch()
        {
            if (!_used)
            {
                _used = true;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            StringBuilder stringBuilder;
            Exception ex;

            stringBuilder = new StringBuilder();
            ex = (Exception)e.ExceptionObject;
            stringBuilder.AppendLine("Message：\n" + ex.Message);
            stringBuilder.AppendLine("Source：\n" + ex.Source);
            stringBuilder.AppendLine("StackTrace：\n" + ex.StackTrace.Trim());
            stringBuilder.AppendLine("TargetSite：\n" + ex.TargetSite);
            MessageBox(IntPtr.Zero, stringBuilder.ToString(), null, 0);
        }
    }
}
