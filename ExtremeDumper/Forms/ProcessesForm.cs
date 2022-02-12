using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Windows.Forms;
using ExtremeDumper.Diagnostics;
using ExtremeDumper.Dumping;
using NativeSharp;

namespace ExtremeDumper.Forms;

partial class ProcessesForm : Form {
	static readonly bool IsAdministrator = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

	readonly StrongBox<DumperType> dumperType = new();
	static bool hasSeDebugPrivilege;

	public ProcessesForm() {
		InitializeComponent();
		Text = $"{Application.ProductName} v{Application.ProductVersion} ({(Environment.Is64BitProcess ? "x64" : "x86")}{(IsAdministrator ? ", Administrator" : string.Empty)})";
		Text = Utils.ObfuscateTitle(Text);
		Utils.EnableDoubleBuffer(lvwProcesses);
		lvwProcesses.ListViewItemSorter = new ListViewItemSorter(lvwProcesses, new[] { TypeCode.String, TypeCode.Int32, TypeCode.String });
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
	void mnuDebugPrivilege_Click(object sender, EventArgs e) {
		if (hasSeDebugPrivilege)
			return;

		if (!IsAdministrator) {
			MessageBoxStub.Show("Please run as administator", MessageBoxIcon.Error);
			return;
		}
		try {
			Process.EnterDebugMode();
			hasSeDebugPrivilege = true;
			mnuDebugPrivilege.Checked = true;
			mnuDebugPrivilege.Enabled = false;
			Text = Text.Substring(0, Text.Length - 1) + ", SeDebugPrivilege)";
			MessageBoxStub.Show("Succeed", MessageBoxIcon.Information);
		}
		catch {
			MessageBoxStub.Show("Failed", MessageBoxIcon.Error);
		}
	}

	void lvwProcesses_Resize(object sender, EventArgs e) {
		lvwProcesses.AutoResizeColumns(true);
	}

	void mnuDumpProcess_Click(object sender, EventArgs e) {
		if (lvwProcesses.SelectedIndices.Count == 0)
			return;

		uint processId = uint.Parse(lvwProcesses.GetFirstSelectedSubItem(chProcessId.Index).Text);
		using var process = NativeProcess.Open(processId);
		fbdlgDumped.SelectedPath = Path.GetDirectoryName(process.ImagePath) + "\\";
		if (fbdlgDumped.ShowDialog() != DialogResult.OK)
			return;
		DumpProcess(processId, Path.Combine(fbdlgDumped.SelectedPath, "Dumps"));
	}

	void mnuViewModules_Click(object sender, EventArgs e) {
		if (lvwProcesses.SelectedIndices.Count == 0)
			return;

		var processNameItem = lvwProcesses.GetFirstSelectedSubItem(chProcessName.Index);
		if (Environment.Is64BitProcess && processNameItem.BackColor == Utils.DotNetColor && processNameItem.Text.EndsWith(" (32 Bit)", StringComparison.Ordinal)) {
			MessageBoxStub.Show("Please run x86 version", MessageBoxIcon.Error);
		}
		else {
			var modulesForm = new ModulesForm(uint.Parse(lvwProcesses.GetFirstSelectedSubItem(chProcessId.Index).Text), processNameItem.Text, processNameItem.BackColor == Utils.DotNetColor, dumperType);
			modulesForm.Show();
		}
	}

	void mnuRefreshProcessList_Click(object sender, EventArgs e) {
		RefreshProcessList();
	}

	void mnuOnlyDotNetProcess_Click(object sender, EventArgs e) {
		RefreshProcessList();
	}

	void mnuInjectDll_Click(object sender, EventArgs e) {
		if (lvwProcesses.SelectedIndices.Count == 0)
			return;

		var injectingForm = new InjectingForm(uint.Parse(lvwProcesses.GetFirstSelectedSubItem(chProcessId.Index).Text));
		injectingForm.Show();
	}

	void mnuGotoLocation_Click(object sender, EventArgs e) {
		if (lvwProcesses.SelectedIndices.Count == 0)
			return;

		Process.Start("explorer.exe", @"/select, " + lvwProcesses.GetFirstSelectedSubItem(chProcessPath.Index).Text);
	}
	#endregion

	void SwitchDumperType(DumperType dumperType) {
		string name = dumperType.ToString();
		foreach (ToolStripMenuItem item in mnuDumperType.DropDownItems)
			item.Checked = item.Text == name;
		this.dumperType.Value = dumperType;
	}

	void RefreshProcessList() {
		lvwProcesses.SuspendLayout();
		lvwProcesses.Items.Clear();
		uint[] processIds = NativeProcess.GetAllProcessIds();
		if (processIds is null)
			return;

		foreach (uint processId in processIds) {
			if (processId == 0)
				continue;
			if (!GetProcessInfo(processId, out var name, out var path, out bool is64Bit, out bool isDotNet))
				continue;
			if (mnuOnlyDotNetProcess.Checked && !isDotNet)
				continue;

			var listViewItem = new ListViewItem(name);
			listViewItem.SubItems.Add(processId.ToString());
			listViewItem.SubItems.Add(path);
			if (Utils.Is64BitProcess && !is64Bit)
				listViewItem.Text += " (32 Bit)";
			if (isDotNet)
				listViewItem.BackColor = Utils.DotNetColor;
			lvwProcesses.Items.Add(listViewItem);
		}
		lvwProcesses.ResumeLayout();
		lvwProcesses.AutoResizeColumns(false);
	}

	static bool GetProcessInfo(uint processId, out string name, out string path, out bool is64Bit, out bool isDotNet) {
		name = string.Empty;
		path = string.Empty;
		is64Bit = false;
		isDotNet = false;

		var modulesProvider = ModulesProviderFactory.Create(processId, ModulesProviderType.Unmanaged);
		var mainModule = modulesProvider.EnumerateModules().FirstOrDefault();
		if (mainModule is null)
			return false;
		// insufficient privileges

		name = mainModule.Name;
		path = mainModule.FilePath;
		var clrModule = modulesProvider.EnumerateModules().FirstOrDefault(t => t.Name.ToUpperInvariant() is "MSCORWKS.DLL" or "CLR.DLL" or "CORECLR.DLL");
		isDotNet = clrModule is not null;
		Is64BitPE(isDotNet ? clrModule!.FilePath : mainModule.FilePath, out is64Bit);
		return true;
	}

	static bool Is64BitPE(string filePath, out bool is64Bit) {
		try {
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var reader = new BinaryReader(stream);
			reader.BaseStream.Position = 0x3C;
			uint peOffset = reader.ReadUInt32();
			reader.BaseStream.Position = peOffset + 0x18;
			ushort magic = reader.ReadUInt16();
			if (magic != 0x010B && magic != 0x020B)
				throw new InvalidDataException();
			is64Bit = magic == 0x020B;
			return true;
		}
		catch {
			is64Bit = false;
			return false;
		}
	}

	void DumpProcess(uint processId, string directoryPath) {
		if (!Directory.Exists(directoryPath))
			Directory.CreateDirectory(directoryPath);
		using var dumper = DumperFactory.GetDumper(processId, dumperType.Value);
		int count = dumper.DumpProcess(directoryPath);
		MessageBoxStub.Show($"{count} images have been dumped to:{Environment.NewLine}{directoryPath}", MessageBoxIcon.Information);
	}
}
