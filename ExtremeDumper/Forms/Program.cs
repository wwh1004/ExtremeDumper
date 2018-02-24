using System;
using System.Windows.Forms;

namespace ExtremeDumper.Forms
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            GlobalExceptionCatcher.Catch();
            if (Cache.Is64BitOperatingSystem && !Environment.Is64BitProcess)
            {
                MessageBoxStub.Show("64位系统下请以64位模式启动" + Application.ProductName, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
