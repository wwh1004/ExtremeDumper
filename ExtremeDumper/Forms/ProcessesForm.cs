using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Windows.Forms;
using ExtremeDumper.Dumping;
using NativeSharp;
using static ExtremeDumper.Forms.NativeMethods;

namespace ExtremeDumper.Forms {
	internal partial class ProcessesForm : Form {
		private static readonly bool _isAdministrator = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
		private readonly StrongBox<DumperType> _dumperType = new();
		private static bool _hasSeDebugPrivilege;

		public ProcessesForm() {
			InitializeComponent();
			Text = $"{Application.ProductName} v{Application.ProductVersion} ({(Environment.Is64BitProcess ? "x64" : "x86")}{(_isAdministrator ? ", Administrator" : string.Empty)})";
			Utils.EnableDoubleBuffer(lvwProcesses);
			lvwProcesses.ListViewItemSorter = new ListViewItemSorter(lvwProcesses, new List<TypeCode> {
				TypeCode.String,
				TypeCode.Int32,
				TypeCode.String
			});
			for (var dumperType = DumperType.Normal; dumperType <= DumperType.Normal; dumperType++) {
				var item = new ToolStripMenuItem(dumperType.ToString());
				var currentDumperType = dumperType;
				item.Click += (object sender, EventArgs e) => SwitchDumperType(currentDumperType);
				mnuDumperType.DropDownItems.Add(item);
			}
			SwitchDumperType(DumperType.Normal);
			RefreshProcessList();
		}

		#region Events
		private void mnuDebugPrivilege_Click(object sender, EventArgs e) {
			if (_hasSeDebugPrivilege)
				return;

			if (!_isAdministrator) {
				MessageBoxStub.Show("Please run as administator", MessageBoxIcon.Error);
				return;
			}
			try {
				Process.EnterDebugMode();
				_hasSeDebugPrivilege = true;
				mnuDebugPrivilege.Checked = true;
				mnuDebugPrivilege.Enabled = false;
				Text = Text.Substring(0, Text.Length - 1) + ", SeDebugPrivilege)";
				MessageBoxStub.Show("Succeed", MessageBoxIcon.Information);
			}
			catch {
				MessageBoxStub.Show("Failed", MessageBoxIcon.Error);
			}
		}

		private void lvwProcesses_Resize(object sender, EventArgs e) {
			lvwProcesses.AutoResizeColumns(true);
		}

		private void mnuDumpProcess_Click(object sender, EventArgs e) {
			if (lvwProcesses.SelectedIndices.Count == 0)
				return;

			uint processId = uint.Parse(lvwProcesses.GetFirstSelectedSubItem(chProcessId.Index).Text);
			using var process = NativeProcess.Open(processId);
			fbdlgDumped.SelectedPath = Path.GetDirectoryName(process.ImagePath);
			if (fbdlgDumped.ShowDialog() != DialogResult.OK)
				return;
			DumpProcess(processId, Path.Combine(fbdlgDumped.SelectedPath, "Dumps"));
		}

		private void mnuViewModules_Click(object sender, EventArgs e) {
			if (lvwProcesses.SelectedIndices.Count == 0)
				return;

			var processNameItem = lvwProcesses.GetFirstSelectedSubItem(chProcessName.Index);
			if (Environment.Is64BitProcess && processNameItem.BackColor == Utils.DotNetColor && processNameItem.Text.EndsWith("(32 Bit)", StringComparison.Ordinal)) {
				MessageBoxStub.Show("Please run x86 version", MessageBoxIcon.Error);
			}
			else {
				var modulesForm = new ModulesForm(uint.Parse(lvwProcesses.GetFirstSelectedSubItem(chProcessId.Index).Text), processNameItem.Text, processNameItem.BackColor == Utils.DotNetColor, _dumperType);
				modulesForm.Show();
			}
		}

		private void mnuRefreshProcessList_Click(object sender, EventArgs e) {
			RefreshProcessList();
		}

		private void mnuOnlyDotNetProcess_Click(object sender, EventArgs e) {
			RefreshProcessList();
		}

		private void mnuInjectDll_Click(object sender, EventArgs e) {
			if (lvwProcesses.SelectedIndices.Count == 0)
				return;

			var injectingForm = new InjectingForm(uint.Parse(lvwProcesses.GetFirstSelectedSubItem(chProcessId.Index).Text));
			injectingForm.Show();
		}

		private void mnuGotoLocation_Click(object sender, EventArgs e) {
			if (lvwProcesses.SelectedIndices.Count == 0)
				return;

			Process.Start("explorer.exe", @"/select, " + lvwProcesses.GetFirstSelectedSubItem(chProcessPath.Index).Text);
		}
		#endregion

		private void SwitchDumperType(DumperType dumperType) {
			string name = dumperType.ToString();
			foreach (ToolStripMenuItem item in mnuDumperType.DropDownItems)
				item.Checked = item.Text == name;
			_dumperType.Value = dumperType;
		}

		private void RefreshProcessList() {
			lvwProcesses.Items.Clear();
			uint[] processIds = NativeProcess.GetAllProcessIds();
			if (processIds is null)
				return;

			var moduleEntry = MODULEENTRY32.Default;
			foreach (uint processId in processIds) {
				if (processId == 0)
					continue;
				var snapshotHandle = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, processId);
				if (snapshotHandle == INVALID_HANDLE_VALUE)
					continue;
				if (!Module32First(snapshotHandle, ref moduleEntry))
					continue;

				var listViewItem = new ListViewItem(moduleEntry.szModule);
				listViewItem.SubItems.Add(processId.ToString());
				listViewItem.SubItems.Add(moduleEntry.szExePath);
				bool isDotNetProcess = false;
				bool is64;
				while (Module32Next(snapshotHandle, ref moduleEntry)) {
					string t;
					if ((t = moduleEntry.szModule.ToUpperInvariant()) == "MSCORWKS.DLL" || t == "CLR.DLL" || t == "CORECLR.DLL") {
						listViewItem.BackColor = Utils.DotNetColor;
						isDotNetProcess = true;
						if (Utils.Is64BitProcess && Is64BitPE(moduleEntry.szExePath, out is64) && !is64)
							listViewItem.Text += "(32 Bit)";
						break;
					}
				}
				if (!isDotNetProcess && Utils.Is64BitProcess && Is64BitPE(listViewItem.SubItems[2].Text, out is64) && !is64)
					listViewItem.Text += "(32 Bit)";
				if (!mnuOnlyDotNetProcess.Checked || isDotNetProcess)
					lvwProcesses.Items.Add(listViewItem);
			}
			lvwProcesses.AutoResizeColumns(false);
		}

		private static bool Is64BitPE(string filePath, out bool is64) {
			try {
				using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var reader = new BinaryReader(stream);
				reader.BaseStream.Position = 0x3C;
				uint peOffset = reader.ReadUInt32();
				reader.BaseStream.Position = peOffset + 0x4;
				ushort machine = reader.ReadUInt16();
				if (machine != 0x14C && machine != 0x8664)
					throw new InvalidDataException();
				is64 = machine == 0x8664;
				return true;
			}
			catch {
				is64 = false;
				return false;
			}
		}

		private void DumpProcess(uint processId, string directoryPath) {
			if (!Directory.Exists(directoryPath))
				Directory.CreateDirectory(directoryPath);
			using var dumper = DumperFactory.GetDumper(processId, _dumperType.Value);
			int count = dumper.DumpProcess(directoryPath);
			MessageBoxStub.Show($"{count} images have been dumped to:{Environment.NewLine}{directoryPath}", MessageBoxIcon.Information);
		}
	}
}
