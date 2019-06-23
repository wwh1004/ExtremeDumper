using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Security.Principal;
using System.Windows.Forms;
using ExtremeDumper.Dumper;
using FastWin32;
using FastWin32.Diagnostics;
using static ExtremeDumper.Forms.NativeMethods;

namespace ExtremeDumper.Forms {
	internal partial class ProcessesForm : Form {
		private DumperCoreWrapper _dumperCore = new DumperCoreWrapper { Value = DumperCore.MegaDumper };

		private ResourceManager _resources = new ResourceManager(typeof(ProcessesForm));

		private static readonly bool _isAdministrator = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

		private static readonly AboutForm _aboutForm = new AboutForm();

		public ProcessesForm() {
			InitializeComponent();
			mnuRequireAdministrator.Checked = _isAdministrator;
			mnuRequireAdministrator.Enabled = !_isAdministrator;
			Text = $"{Application.ProductName} v{Application.ProductVersion} ({(Environment.Is64BitProcess ? "x64" : "x86")}{(_isAdministrator ? _resources.GetString("StrAdministrator") : string.Empty)})";
			typeof(ListView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, lvwProcesses, new object[] { true });
			lvwProcesses.ListViewItemSorter = new ListViewItemSorter(lvwProcesses, new Dictionary<int, TypeCode> { { 0, TypeCode.String }, { 1, TypeCode.Int32 }, { 2, TypeCode.String } });
			RefreshProcessList();
		}

		#region Events
		private void mnuRequireAdministrator_Click(object sender, EventArgs e) => Process32.SelfElevate(Handle);

		private void mnuDebugPrivilege_Click(object sender, EventArgs e) {
			if (!_isAdministrator) {
				MessageBoxStub.Show(_resources.GetString("StrRunAsAdmin") + Application.ProductName, MessageBoxIcon.Error);
				return;
			}
			if (FastWin32Settings.EnableDebugPrivilege()) {
				mnuDebugPrivilege.Checked = true;
				mnuDebugPrivilege.Enabled = false;
				Text = Text.Substring(0, Text.Length - 1) + ", SeDebugPrivilege)";
				MessageBoxStub.Show(_resources.GetString("StrSuccess"), MessageBoxIcon.Information);
			}
			else
				MessageBoxStub.Show(_resources.GetString("StrFailed"), MessageBoxIcon.Error);
		}

		private void mnuUseMegaDumper_Click(object sender, EventArgs e) => SwitchDumperCore(DumperCore.MegaDumper);

		private void mnuUseMetaDumper_Click(object sender, EventArgs e) => SwitchDumperCore(DumperCore.MetaDumper);

		private void mnuAbout_Click(object sender, EventArgs e) => _aboutForm.ShowDialog();

		private void lvwProcesses_Resize(object sender, EventArgs e) => lvwProcesses.AutoResizeColumns(true);

		private void mnuDumpProcess_Click(object sender, EventArgs e) {
			if (lvwProcesses.SelectedIndices.Count == 0)
				return;

			if (fbdlgDumped.ShowDialog() != DialogResult.OK)
				return;
			DumpProcess(uint.Parse(lvwProcesses.SelectedItems[0].SubItems[1].Text), Path.Combine(fbdlgDumped.SelectedPath, "Dumps"));
		}

		private void mnuViewModules_Click(object sender, EventArgs e) {
			if (lvwProcesses.SelectedIndices.Count == 0)
				return;

			if (Environment.Is64BitProcess && lvwProcesses.SelectedItems[0].BackColor == Cache.DotNetColor && lvwProcesses.SelectedItems[0].Text.EndsWith(_resources.GetString("Str32Bit"), StringComparison.Ordinal))
				MessageBoxStub.Show(_resources.GetString("StrViewModulesSwitchTo32Bit"), MessageBoxIcon.Error);
			else
				new ModulesForm(uint.Parse(lvwProcesses.SelectedItems[0].SubItems[1].Text), lvwProcesses.SelectedItems[0].Text, lvwProcesses.SelectedItems[0].BackColor == Cache.DotNetColor, _dumperCore).Show();
		}

		private void mnuRefreshProcessList_Click(object sender, EventArgs e) => RefreshProcessList();

		private void mnuOnlyDotNetProcess_Click(object sender, EventArgs e) => RefreshProcessList();

		private void mnuInjectDll_Click(object sender, EventArgs e) {
			if (lvwProcesses.SelectedIndices.Count == 0)
				return;

			new InjectingForm(uint.Parse(lvwProcesses.SelectedItems[0].SubItems[1].Text), lvwProcesses.SelectedItems[0].Text).Show();
		}

		private void mnuGotoLocation_Click(object sender, EventArgs e) {
			if (lvwProcesses.SelectedIndices.Count == 0)
				return;

			Process.Start("explorer.exe", @"/select, " + lvwProcesses.SelectedItems[0].SubItems[2].Text);
		}
		#endregion

		private void SwitchDumperCore(DumperCore dumperCore) {
			mnuUseMegaDumper.Checked = false;
			mnuUseMetaDumper.Checked = false;
			switch (dumperCore) {
			case DumperCore.MegaDumper:
				_dumperCore.Value = DumperCore.MegaDumper;
				mnuUseMegaDumper.Checked = true;
				break;
			case DumperCore.MetaDumper:
				_dumperCore.Value = DumperCore.MetaDumper;
				mnuUseMetaDumper.Checked = true;
				break;
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		private void RefreshProcessList() {
			uint[] processIds;
			IntPtr snapshotHandle;
			MODULEENTRY32 moduleEntry32;
			ListViewItem listViewItem;
			string t;
			bool isDotNetProcess;
			bool is64;

			lvwProcesses.Items.Clear();
			processIds = Process32.GetAllProcessIds();
			if (processIds == null)
				return;
			moduleEntry32 = MODULEENTRY32.Default;
			foreach (uint processId in processIds) {
				if (processId == 0)
					continue;
				snapshotHandle = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, processId);
				if (snapshotHandle == INVALID_HANDLE_VALUE)
					continue;
				if (!Module32First(snapshotHandle, ref moduleEntry32))
					continue;
				listViewItem = new ListViewItem(moduleEntry32.szModule);
				listViewItem.SubItems.Add(processId.ToString());
				listViewItem.SubItems.Add(moduleEntry32.szExePath);
				isDotNetProcess = false;
				while (Module32Next(snapshotHandle, ref moduleEntry32))
					if ((t = moduleEntry32.szModule.ToUpperInvariant()) == "MSCORJIT.DLL" || t == "MSCOREE.DLL" || t == "MSCORWKS.DLL" || t == "CLR.DLL" || t == "CLRJIT.DLL") {
						listViewItem.BackColor = Cache.DotNetColor;
						isDotNetProcess = true;
						if (Cache.Is64BitProcess && Is64BitPE(moduleEntry32.szExePath, out is64) && !is64)
							listViewItem.Text += _resources.GetString("Str32Bit");
						break;
					}
				if (Cache.Is64BitProcess && !isDotNetProcess && Is64BitPE(listViewItem.SubItems[2].Text, out is64) && !is64)
					listViewItem.Text += _resources.GetString("Str32Bit");
				if (!mnuOnlyDotNetProcess.Checked || isDotNetProcess)
					lvwProcesses.Items.Add(listViewItem);
			}
			lvwProcesses.AutoResizeColumns(false);
		}

		private static bool Is64BitPE(string filePath, out bool is64) {
			FileStream fileStream;
			BinaryReader binaryReader;
			uint peOffset;
			ushort machine;

			try {
				using (fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					binaryReader = new BinaryReader(fileStream);
					binaryReader.BaseStream.Position = 0x3C;
					peOffset = binaryReader.ReadUInt32();
					binaryReader.BaseStream.Position = peOffset + 0x4;
					machine = binaryReader.ReadUInt16();
					if (machine != 0x14C && machine != 0x8664)
						throw new InvalidDataException();
					is64 = machine == 0x8664;
				}
				return true;
			}
			catch {
				is64 = false;
				return false;
			}
		}

		private void DumpProcess(uint processId, string directoryPath) => MessageBoxStub.Show($"{DumperFactory.GetDumper(processId, _dumperCore.Value).DumpProcess(directoryPath).ToString()} {_resources.GetString("StrDumpFilesSuccess")}{Environment.NewLine}{directoryPath}", MessageBoxIcon.Information);
	}
}
