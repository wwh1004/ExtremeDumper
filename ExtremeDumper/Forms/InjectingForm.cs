using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using dnlib.DotNet;
using ExtremeDumper.Diagnostics;
using ExtremeDumper.Injecting;
using ExtremeDumper.Logging;

namespace ExtremeDumper.Forms;

partial class InjectingForm : Form {
	readonly ProcessInfo process;
	string assemblyPath = string.Empty;
	ModuleDef? module;
	MethodDef? entryPoint;
	string argument = string.Empty;

	public InjectingForm(ProcessInfo process) {
		this.process = process;
		InitializeComponent();
		Text = TitleComposer.Compose(true, "Injector", process.Name, null, $"ID={process.Id}");
		cmbCLRVersion.SelectedIndex = 0;
	}

	#region Events
	void InjectingForm_DragEnter(object sender, DragEventArgs e) {
		e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
	}

	void InjectingForm_DragDrop(object sender, DragEventArgs e) {
		tbAssemblyPath.Text = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
		LoadAssembly();
	}

	void tbAssemblyPath_TextChanged(object sender, EventArgs e) {
		assemblyPath = tbAssemblyPath.Text;
	}

	void btSelectAssembly_Click(object sender, EventArgs e) {
		if (odlgSelectAssembly.ShowDialog() == DialogResult.OK)
			tbAssemblyPath.Text = odlgSelectAssembly.FileName;
		else
			return;
		LoadAssembly();
	}

	void cmbEntryPoint_SelectedIndexChanged(object sender, EventArgs e) {
		entryPoint = (MethodDef)cmbEntryPoint.SelectedItem;
	}

	void tbArgument_TextChanged(object sender, EventArgs e) {
		argument = tbArgument.Text;
	}

	void btInject_Click(object sender, EventArgs e) {
		if (!File.Exists(assemblyPath))
			return;
		if (entryPoint is null)
			return;

		string typeName = entryPoint.FullName.Substring(entryPoint.FullName.IndexOf(' ') + 1);
		typeName = typeName.Substring(0, typeName.IndexOf(':'));
		var clrVersion = cmbCLRVersion.SelectedItem switch {
			"CLR 2.x" => InjectionClrVersion.V2,
			"CLR 4.x" => InjectionClrVersion.V4,
			_ => throw new NotSupportedException()
		};
		if (chkWaitReturn.Checked) {
			btInject.Enabled = false;
			Text += "Waiting...";
			new Thread(() => {
				if (Injector.InjectManagedAndWait(process.Id, assemblyPath, typeName, entryPoint.Name, argument, clrVersion, out int ret))
					Logger.Info($"Inject successfully and return value is {Formatter.FormatHex(ret)}");
				else
					Logger.Error("Failed to inject");
				Invoke(() => {
					btInject.Enabled = true;
					Text = Text.Substring(0, Text.Length - 6);
				});
			}) { IsBackground = true }.Start();
		}
		else {
			if (Injector.InjectManaged(process.Id, assemblyPath, typeName, entryPoint.Name, argument, clrVersion))
				Logger.Info("Inject successfully");
			else
				Logger.Error("Failed to inject");
		}
	}
	#endregion

	void LoadAssembly() {
		try {
			module = ModuleDefMD.Load(File.ReadAllBytes(assemblyPath));
		}
		catch {
			Logger.Error("Invalid assembly");
			module = null;
			return;
		}
		cmbEntryPoint.Items.Clear();
		foreach (var type in module.GetTypes()) {
			foreach (var method in type.Methods) {
				if (!method.IsStatic)
					continue;
				if (method.IsGetter || method.IsSetter)
					continue;

				var methodSig = (MethodSig)method.Signature;
				if (methodSig.Params.Count != 1 || methodSig.Params[0].FullName != "System.String")
					continue;
				if (methodSig.RetType.FullName != "System.Int32")
					continue;

				cmbEntryPoint.Items.Add(method);
			}
		}
		if (cmbEntryPoint.Items.Count == 1)
			cmbEntryPoint.SelectedIndex = 0;
		cmbCLRVersion.SelectedIndex = module.CorLibTypes.AssemblyRef.Version.Major == 4 ? 1 : 0;
	}

	protected override void Dispose(bool disposing) {
		if (disposing) {
			components?.Dispose();
			module?.Dispose();
		}
		base.Dispose(disposing);
	}
}
