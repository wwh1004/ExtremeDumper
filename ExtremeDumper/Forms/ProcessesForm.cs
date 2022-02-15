using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExtremeDumper.Diagnostics;
using ExtremeDumper.Dumping;

namespace ExtremeDumper.Forms;

partial class ProcessesForm : Form {
	static readonly bool IsAdministrator = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
	static bool hasSeDebugPrivilege;

	readonly StrongBox<DumperType> dumperType = new();
	readonly List<ProcessInfo> processes = new();
	readonly TitleComposer title;

	public ProcessesForm() {
		InitializeComponent();
		title = new TitleComposer {
			Title = Application.ProductName,
			Version = $"v{Application.ProductVersion}"
		};
		title.Annotations["BITNESS"] = Environment.Is64BitProcess ? "x64" : "x86";
		if (IsAdministrator)
			title.Annotations["ADMIN"] = "Administrator";
		Text = title.Compose(true);
		Utils.EnableDoubleBuffer(lvwProcesses);
		lvwProcesses.ListViewItemSorter = new ListViewItemSorter(lvwProcesses, new[] { TypeCode.String, TypeCode.Int32, TypeCode.String });
		for (var dumperType = DumperType.Normal; dumperType <= DumperType.AntiAntiDump; dumperType++) {
			var item = new ToolStripMenuItem(dumperType.ToString());
			var currentDumperType = dumperType;
			item.Click += (_, _) => SwitchDumperType(currentDumperType);
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
			title.Annotations["SE_DEBUG"] = "SeDebugPrivilege";
			Text = title.Compose(true);
			MessageBoxStub.Show("Succeed", MessageBoxIcon.Information);
		}
		catch {
			MessageBoxStub.Show("Failed", MessageBoxIcon.Error);
		}
	}

	void lvwProcesses_Resize(object sender, EventArgs e) {
		lvwProcesses.AutoResizeColumns(true);
	}

	async void mnuDumpProcess_Click(object sender, EventArgs e) {
		if (!TryGetSelectedProcess(out var process))
			return;

		try {
			mnuDumpProcess.Enabled = false;
			title.Annotations["DUMP"] = "Dumping";
			Text = title.Compose(true);
			var path = Path.GetDirectoryName(process.FilePath);
			if (!mnuFastDump.Checked) {
				fbdlgDumped.SelectedPath = path + "\\";
				if (fbdlgDumped.ShowDialog() != DialogResult.OK)
					return;
				path = fbdlgDumped.SelectedPath;
			}
			int count = await Task.Run(() => DumpProcess(process.Id, Path.Combine(path, "Dumps")));
			MessageBoxStub.Show($"{count} images have been dumped to:{Environment.NewLine}{path}", MessageBoxIcon.Information);
		}
		finally {
			title.Annotations["DUMP"] = null;
			Text = title.Compose(true);
			mnuDumpProcess.Enabled = true;
		}
	}

	void mnuViewModules_Click(object sender, EventArgs e) {
		if (!TryGetSelectedProcess(out var process))
			return;

		if (Utils.Is64BitProcess && process is DotNetProcessInfo && !process.Is64Bit) {
			MessageBoxStub.Show("Please run x86 version", MessageBoxIcon.Error);
			return;
		}

		var form = ModulesForm.Create(process, dumperType);
		if (form is not null)
			form.Show();
		else
			MessageBoxStub.Show($"Can't create {nameof(ModulesForm)}", MessageBoxIcon.Error);
	}

	void mnuRefreshProcessList_Click(object sender, EventArgs e) {
		try {
			title.Annotations["REFRESH"] = "Refreshing";
			Text = title.Compose(true);
			RefreshProcessList();
		}
		finally {
			title.Annotations["REFRESH"] = null;
			Text = title.Compose(true);
		}
	}

	void mnuOnlyDotNetProcess_Click(object sender, EventArgs e) {
		mnuRefreshProcessList_Click(sender, e);
	}

	void mnuInjectDll_Click(object sender, EventArgs e) {
		if (!TryGetSelectedProcess(out var process))
			return;

		var form = InjectingForm.Create(process);
		if (form is not null)
			form.Show();
		else
			MessageBoxStub.Show($"Can't create {nameof(InjectingForm)}", MessageBoxIcon.Error);
	}

	void mnuGotoLocation_Click(object sender, EventArgs e) {
		if (!TryGetSelectedProcess(out var process))
			return;

		Process.Start("explorer.exe", $"/select,{process.FilePath}");
	}
	#endregion

	bool TryGetSelectedProcess([NotNullWhen(true)] out ProcessInfo? process) {
		process = null;
		if (lvwProcesses.SelectedIndices.Count == 0)
			return false;

		uint processId = uint.Parse(lvwProcesses.GetFirstSelectedSubItem(chProcessId.Index).Text);
		process = processes.Find(t => t.Id == processId);
		Debug2.Assert(process is not null);
		return true;
	}

	void SwitchDumperType(DumperType dumperType) {
		string name = dumperType.ToString();
		foreach (ToolStripMenuItem item in mnuDumperType.DropDownItems)
			item.Checked = item.Text == name;
		this.dumperType.Value = dumperType;
	}

	void RefreshProcessList() {
		Utils.RefreshListView(lvwProcesses, GetProcesses(), t => CreateListViewItem(t), 10);
	}

	IEnumerable<ProcessInfo> GetProcesses() {
		processes.Clear();
		foreach (var process in ProcessesProviderFactory.Create().EnumerateProcesses()) {
			if (mnuOnlyDotNetProcess.Checked && process is not DotNetProcessInfo)
				continue;
			processes.Add(process);
			yield return process;
		}
	}

	static ListViewItem CreateListViewItem(ProcessInfo process) {
		var listViewItem = new ListViewItem(process.Name);
		// Name
		listViewItem.SubItems.Add(process.Id.ToString());
		// Id
		if (process is DotNetProcessInfo dnProcess)
			listViewItem.SubItems.Add(string.Join(", ", dnProcess.CLRModules.Select(t => t.Name)));
		else
			listViewItem.SubItems.Add(string.Empty);
		// CLR
		listViewItem.SubItems.Add(process.FilePath);
		// Path
		if (Utils.Is64BitProcess && !process.Is64Bit)
			listViewItem.Text += " (32 Bit)";
		if (process is DotNetProcessInfo)
			listViewItem.BackColor = Utils.DotNetColor;
		return listViewItem;
	}

	int DumpProcess(uint processId, string directoryPath) {
		if (!Directory.Exists(directoryPath))
			Directory.CreateDirectory(directoryPath);
		using var dumper = DumperFactory.Create(processId, dumperType.Value);
		return dumper.DumpProcess(directoryPath);
	}
}
