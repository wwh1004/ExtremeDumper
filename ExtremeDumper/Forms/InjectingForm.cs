using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using dnlib.DotNet;
using NativeSharp;

namespace ExtremeDumper.Forms {
	internal partial class InjectingForm : Form {
		private readonly NativeProcess _process;
		private string _assemblyPath;
		private ModuleDef _module;
		private MethodDef _entryPoint;
		private string _argument;

		public InjectingForm(uint processId) {
			InitializeComponent();
			_process = NativeProcess.Open(processId);
			if (_process == NativeProcess.InvalidProcess)
				throw new InvalidOperationException();
			Text = $"Injector - {_process.Name}(ID={_process.Id})";
		}

		#region Events
		private void InjectingForm_DragEnter(object sender, DragEventArgs e) {
			e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
		}

		private void InjectingForm_DragDrop(object sender, DragEventArgs e) {
			tbAssemblyPath.Text = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
			LoadAssembly();
		}

		private void tbAssemblyPath_TextChanged(object sender, EventArgs e) {
			_assemblyPath = tbAssemblyPath.Text;
		}

		private void btSelectAssembly_Click(object sender, EventArgs e) {
			if (odlgSelectAssembly.ShowDialog() == DialogResult.OK)
				tbAssemblyPath.Text = odlgSelectAssembly.FileName;
			else
				return;
			LoadAssembly();
		}

		private void cmbEntryPoint_SelectedIndexChanged(object sender, EventArgs e) {
			_entryPoint = (MethodDef)cmbEntryPoint.SelectedItem;
		}

		private void tbArgument_TextChanged(object sender, EventArgs e) {
			_argument = tbArgument.Text;
		}

		private void btInject_Click(object sender, EventArgs e) {
			if (!File.Exists(_assemblyPath))
				return;
			if (cmbEntryPoint.SelectedItem is null)
				return;

			string typeName = _entryPoint.FullName.Substring(_entryPoint.FullName.IndexOf(' ') + 1);
			typeName = typeName.Substring(0, typeName.IndexOf(':'));
			if (chkWaitReturn.Checked) {
				btInject.Enabled = false;
				Text += "Waiting...";
				new Thread(() => {
					if (_process.InjectManaged(_assemblyPath, typeName, _entryPoint.Name, _argument, out int ret))
						Invoke((Action)(() => MessageBoxStub.Show($"Inject successfully and return value is {ret}", MessageBoxIcon.Information)));
					else
						Invoke((Action)(() => MessageBoxStub.Show("Failed to inject", MessageBoxIcon.Error)));
					Invoke((Action)(() => {
						btInject.Enabled = true;
						Text = Text.Substring(0, Text.Length - 6);
					}));
				}) { IsBackground = true }.Start();
			}
			else {
				if (_process.InjectManaged(_assemblyPath, typeName, _entryPoint.Name, _argument))
					MessageBoxStub.Show("Inject successfully", MessageBoxIcon.Information);
				else
					MessageBoxStub.Show("Failed to inject", MessageBoxIcon.Error);
			}
		}
		#endregion

		private void LoadAssembly() {
			try {
				_module = ModuleDefMD.Load(File.ReadAllBytes(_assemblyPath));
			}
			catch {
				MessageBoxStub.Show("Invalid assembly", MessageBoxIcon.Error);
				_module = null;
				return;
			}
			cmbEntryPoint.Items.Clear();
			foreach (var typeDef in _module.GetTypes()) {
				foreach (var methodDef in typeDef.Methods) {
					if (!methodDef.IsStatic)
						continue;
					if (methodDef.IsGetter || methodDef.IsSetter)
						continue;

					var methodSig = (MethodSig)methodDef.Signature;
					if (methodSig.Params.Count != 1 || methodSig.Params[0].FullName != "System.String")
						continue;
					if (methodSig.RetType.FullName != "System.Int32")
						continue;

					cmbEntryPoint.Items.Add(methodDef);
				}
			}
			if (cmbEntryPoint.Items.Count == 1)
				cmbEntryPoint.SelectedIndex = 0;
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				components?.Dispose();
				_module?.Dispose();
				_process.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
