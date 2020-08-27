using System;
using System.Windows.Forms;
using ExtremeDumper.Forms;

namespace ExtremeDumper {
	public static class Program {
		[STAThread]
		public static void Main() {
			Environment.SetEnvironmentVariable("_NT_SYMBOL_PATH", string.Empty);
			// 禁止在线搜索PDB文件
			GlobalExceptionCatcher.Catch();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ProcessesForm());
		}
	}
}
