using System;
using System.Windows.Forms;
using ExtremeDumper.Forms;
using Microsoft.Diagnostics.Runtime.Utilities;

namespace ExtremeDumper {
	public static class Program {
		[STAThread]
		public static void Main() {
			SymbolLocator._NT_SYMBOL_PATH = " ";
			// 禁止在线搜索PDB文件
			GlobalExceptionCatcher.Catch();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ProcessesForm());
		}
	}
}
