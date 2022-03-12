using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ExtremeDumper.Forms;

partial class LoaderHookForm : Form {
	[DllImport("ExtremeDumper.LoaderHook.dll", BestFitMapping = false, CharSet = CharSet.Unicode, SetLastError = true)]
	static extern uint LoaderHookCreateProcess(string applicationName, StringBuilder? commandLine);

	string assemblyPath = string.Empty;

	public LoaderHookForm() {
		InitializeComponent();
	}

	#region Events
	void LoaderHookForm_DragEnter(object sender, DragEventArgs e) {
		e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
	}

	void LoaderHookForm_DragDrop(object sender, DragEventArgs e) {
		tbAssemblyPath.Text = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
	}

	void tbAssemblyPath_TextChanged(object sender, EventArgs e) {
		assemblyPath = tbAssemblyPath.Text;
	}

	void btSelectAssembly_Click(object sender, EventArgs e) {
		if (odlgSelectAssembly.ShowDialog() == DialogResult.OK)
			tbAssemblyPath.Text = odlgSelectAssembly.FileName;
		else
			return;
	}

	void btnRun_Click(object sender, EventArgs e) {
		if (string.IsNullOrEmpty(assemblyPath))
			return;

		uint hr = LoaderHookCreateProcess(assemblyPath, null);
		MessageBoxStub.Show(hr == 0 ? "Succeed" : $"Failed, try ExtremeDumper{(IntPtr.Size == 4 ? string.Empty : "-x86")}.exe", hr == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Error);
	}
	#endregion
}
