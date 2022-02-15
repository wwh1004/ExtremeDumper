using System;
using System.Windows.Forms;
using ExtremeDumper.Forms;

namespace ExtremeDumper;

public static class Program {
	[STAThread]
	public static void Main() {
		GlobalExceptionCatcher.Catch();
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		Application.Run(new ProcessesForm());
	}
}
