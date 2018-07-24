using System;
using System.Windows.Forms;

namespace ExtremeDumper.Forms
{
    public static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        public static void Main()
        {
            GlobalExceptionCatcher.Catch();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ProcessesForm());
        }
    }
}
