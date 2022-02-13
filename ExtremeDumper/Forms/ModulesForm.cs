using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using ExtremeDumper.Diagnostics;
using ExtremeDumper.Dumping;
using NativeSharp;
using ImageLayout = dnlib.PE.ImageLayout;

namespace ExtremeDumper.Forms;

partial class ModulesForm : Form {
	static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

	readonly NativeProcess process;
	readonly bool isDotNet;
	readonly StrongBox<DumperType> dumperType;

	public ModulesForm(uint processId, string processName, bool isDotNet, StrongBox<DumperType> dumperType) {
		InitializeComponent();
		process = NativeProcess.Open(processId);
		if (process.IsInvalid)
			throw new InvalidOperationException();
		this.isDotNet = isDotNet;
		this.dumperType = dumperType;
		Text = TitleComposer.Compose(true, "Modules", processName, null, $"ID={processId}");
		Utils.EnableDoubleBuffer(lvwModules);
		lvwModules.ListViewItemSorter = new ListViewItemSorter(lvwModules, new[] { TypeCode.String, TypeCode.String, TypeCode.String, TypeCode.UInt64, TypeCode.Int32, TypeCode.String }) { AllowHexLeading = true };
		RefreshModuleList();
	}

	#region Events
	void lvwModules_Resize(object sender, EventArgs e) {
		lvwModules.AutoResizeColumns(true);
	}

	void mnuDumpModule_Click(object sender, EventArgs e) {
		if (lvwModules.SelectedIndices.Count == 0)
			return;

		string filePath = EnsureValidFileName(lvwModules.GetFirstSelectedSubItem(chModuleName.Index).Text);
		if (filePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || filePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
			filePath = PathInsertPostfix(filePath, ".dump");
		else
			filePath += ".dump.dll";
		sfdlgDumped.FileName = filePath;
		sfdlgDumped.InitialDirectory = Path.GetDirectoryName(process.GetMainModule().ImagePath);
		if (sfdlgDumped.ShowDialog() != DialogResult.OK)
			return;
		var moduleHandle = (nuint)ulong.Parse(lvwModules.GetFirstSelectedSubItem(chModuleHandle.Index).Text.Substring(2), NumberStyles.HexNumber, null);
		DumpModule(moduleHandle, lvwModules.GetFirstSelectedSubItem(chModulePath.Index).Text == "InMemory" ? ImageLayout.File : ImageLayout.Memory, sfdlgDumped.FileName);
	}

	void mnuRefreshModuleList_Click(object sender, EventArgs e) {
		RefreshModuleList();
	}

	unsafe void mnuViewFunctions_Click(object sender, EventArgs e) {
		if (lvwModules.SelectedIndices.Count == 0)
			return;

		var functionsForm = new FunctionsForm(process.UnsafeGetModule((void*)ulong.Parse(lvwModules.GetFirstSelectedSubItem(chModuleHandle.Index).Text.Substring(2), NumberStyles.HexNumber, null)));
		functionsForm.Show();
	}

	void mnuOnlyDotNetModule_Click(object sender, EventArgs e) {
		RefreshModuleList();
	}

	void mnuGotoLocation_Click(object sender, EventArgs e) {
		if (lvwModules.SelectedIndices.Count == 0)
			return;

		string filePath = lvwModules.GetFirstSelectedSubItem(chModulePath.Index).Text;
		if (filePath != "InMemory")
			Process.Start("explorer.exe", @"/select, " + filePath);
	}
	#endregion

	void RefreshModuleList() {
		lvwModules.SuspendLayout();
		lvwModules.Items.Clear();

		var dnModules = Array.Empty<ModuleInfo>();
		if (isDotNet) {
			try {
				dnModules = ModulesProviderFactory.Create(process.Id, ModulesProviderType.Managed).EnumerateModules().ToArray();
			}
			catch {
				MessageBoxStub.Show("Fail to get .NET modules", MessageBoxIcon.Error);
			}
			lvwModules.Items.AddRange(dnModules.Select(t => CreateListViewItem(t)).ToArray());
		}

		if (!mnuOnlyDotNetModule.Checked) {
			var modules = ModulesProviderFactory.Create(process.Id, ModulesProviderType.Unmanaged).EnumerateModules().Where(x => !dnModules.Any(y => x.Name == y.Name && x.ImageBase == y.ImageBase)).ToArray();
			lvwModules.Items.AddRange(modules.Select(t => CreateListViewItem(t)).ToArray());
		}

		lvwModules.ResumeLayout();
		lvwModules.AutoResizeColumns(false);
	}

	static ListViewItem CreateListViewItem(ModuleInfo module) {
		var listViewItem = new ListViewItem(module.Name);
		// Name
		if (module is DotNetModuleInfo dnModule) {
			listViewItem.SubItems.Add(dnModule.DomainName);
			// Domain Name
			listViewItem.SubItems.Add(dnModule.CLRVersion);
			// CLR Version
			listViewItem.BackColor = Utils.DotNetColor;
		}
		else {
			listViewItem.SubItems.Add(string.Empty);
			// Domain Name
			listViewItem.SubItems.Add(string.Empty);
			// CLR Version
		}
		listViewItem.SubItems.Add(Utils.FormatPointer(module.ImageBase));
		// Address
		listViewItem.SubItems.Add(Utils.FormatHex(module.ImageSize));
		// Size
		listViewItem.SubItems.Add(module.FilePath);
		// Path
		return listViewItem;
	}

	static string EnsureValidFileName(string fileName) {
		if (string.IsNullOrEmpty(fileName))
			return string.Empty;

		var newFileName = new StringBuilder(fileName.Length);
		foreach (char chr in fileName) {
			if (!InvalidFileNameChars.Contains(chr))
				newFileName.Append(chr);
		}
		return newFileName.ToString();
	}

	void DumpModule(nuint moduleHandle, ImageLayout imageLayout, string filePath) {
		using var dumper = DumperFactory.GetDumper(process.Id, dumperType.Value);
		bool result = dumper.DumpModule(moduleHandle, imageLayout, filePath);
		if (result)
			MessageBoxStub.Show($"Dump module successfully. Image was saved in:{Environment.NewLine}{filePath}", MessageBoxIcon.Information);
		else
			MessageBoxStub.Show("Fail to dump module.", MessageBoxIcon.Error);
	}

	static string PathInsertPostfix(string path, string postfix) {
		return Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + postfix + Path.GetExtension(path));
	}

	protected override void Dispose(bool disposing) {
		if (disposing) {
			components?.Dispose();
			process.Dispose();
		}
		base.Dispose(disposing);
	}
}
