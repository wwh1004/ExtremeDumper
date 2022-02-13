using System;
using System.Diagnostics;
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

	readonly TitleComposer title;
	readonly StrongBox<DumperType> dumperType = new();
	static bool hasSeDebugPrivilege;

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
		if (lvwProcesses.SelectedIndices.Count == 0)
			return;

		uint processId = uint.Parse(lvwProcesses.GetFirstSelectedSubItem(chProcessId.Index).Text);
		var processPath = lvwProcesses.GetFirstSelectedSubItem(chProcessPath.Index).Text;
		fbdlgDumped.SelectedPath = Path.GetDirectoryName(processPath) + "\\";
		if (fbdlgDumped.ShowDialog() != DialogResult.OK)
			return;
		int count = await Task.Run(() => DumpProcess(processId, Path.Combine(fbdlgDumped.SelectedPath, "Dumps")));
		MessageBoxStub.Show($"{count} images have been dumped to:{Environment.NewLine}{fbdlgDumped.SelectedPath}", MessageBoxIcon.Information);
	}

	void mnuViewModules_Click(object sender, EventArgs e) {
		if (lvwProcesses.SelectedIndices.Count == 0)
			return;

		var processNameItem = lvwProcesses.GetFirstSelectedSubItem(chProcessName.Index);
		if (Environment.Is64BitProcess && processNameItem.BackColor == Utils.DotNetColor && processNameItem.Text.EndsWith(" (32 Bit)", StringComparison.Ordinal)) {
			MessageBoxStub.Show("Please run x86 version", MessageBoxIcon.Error);
			return;
		}

		var modulesForm = new ModulesForm(uint.Parse(lvwProcesses.GetFirstSelectedSubItem(chProcessId.Index).Text), processNameItem.Text, processNameItem.BackColor == Utils.DotNetColor, dumperType);
		modulesForm.Show();
		modulesForm.FormClosed += (sender, e) => (sender as IDisposable)?.Dispose();
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
		Utils.RefreshListView(lvwProcesses,
			ProcessesProviderFactory.Create().EnumerateProcesses().Where(t => !mnuOnlyDotNetProcess.Checked || t is DotNetProcessInfo),
			t => CreateListViewItem(t), 10);
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
