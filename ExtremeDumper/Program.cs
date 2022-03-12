using System;
using System.Windows.Forms;
using ExtremeDumper.Forms;

namespace ExtremeDumper;

public static class Program {
	[STAThread]
	public static void Main() {
		Environment.SetEnvironmentVariable("EXTREMEDUMPER_MAGIC", "C41F3A60");
		CosturaUtility.Initialize();
		Environment.SetEnvironmentVariable("EXTREMEDUMPER_MAGIC", null);
		Console.Title = string.Empty;
		GlobalExceptionCatcher.Catch();
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		Application.Run(new ProcessesForm());
	}
}
