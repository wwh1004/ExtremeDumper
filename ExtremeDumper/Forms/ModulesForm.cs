using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExtremeDumper.Diagnostics;
using ExtremeDumper.Dumping;
using ExtremeDumper.Logging;
using ImageLayout = dnlib.PE.ImageLayout;

namespace ExtremeDumper.Forms;

partial class ModulesForm : Form {
	static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

	readonly ProcessInfo process;
	readonly StrongBox<DumperType> dumperType;
	readonly TitleComposer title;
	readonly List<ModuleInfo> modules = new();

	public ModulesForm(ProcessInfo process, StrongBox<DumperType> dumperType) {
		InitializeComponent();
		Utils.ScaleByDpi(this);
		this.process = process;
		this.dumperType = dumperType;
		title = new TitleComposer {
			Title = "Modules",
			Subtitle = process.Name
		};
		title.Annotations["PID"] = $"PID={process.Id}";
		Text = title.Compose(true);
		Utils.EnableDoubleBuffer(lvwModules);
		lvwModules.ListViewItemSorter = new ListViewItemSorter(lvwModules, new[] { TypeCode.String, TypeCode.String, TypeCode.String, TypeCode.UInt64, TypeCode.Int32, TypeCode.String }) { AllowHexLeading = true };
		RefreshModuleList();
	}

	#region Events
	void lvwModules_Resize(object sender, EventArgs e) {
		lvwModules.AutoResizeColumns(true);
	}

	async void mnuDumpModule_Click(object sender, EventArgs e) {
		if (!TryGetSelectedModule(out var module))
			return;

		try {
			mnuDumpModule.Enabled = false;
			title.Annotations["DUMP"] = "Dumping";
			Text = title.Compose(true);

			string filePath = EnsureValidFileName(module.Name);
			if (filePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || filePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
				filePath = PathInsertPostfix(filePath, ".dump");
			else
				filePath += ".dump.dll";
			sfdlgDumped.FileName = filePath;
			sfdlgDumped.InitialDirectory = Path.GetDirectoryName(process.FilePath);
			if (sfdlgDumped.ShowDialog() != DialogResult.OK)
				return;

			var imageLayout = module is DotNetModuleInfo dnModule && dnModule.InMemory ? ImageLayout.File : ImageLayout.Memory;
			bool result = await Task.Run(() => DumpModule(module.ImageBase, imageLayout, sfdlgDumped.FileName));
			if (result)
				Logger.Info($"Dump module successfully. Image was saved to: {sfdlgDumped.FileName}");
			else
				Logger.Error("Fail to dump module.");
		}
		catch (Exception ex) {
			Logger.Error("Exception occurred while dumping module");
			Logger.Exception(ex);
		}
		finally {
			title.Annotations["DUMP"] = null;
			Text = title.Compose(true);
			mnuDumpModule.Enabled = true;
		}
	}

	async void mnuRefreshModuleList_Click(object sender, EventArgs e) {
		try {
			mnuRefreshModuleList.Enabled = false;
			mnuOnlyDotNetModule.Enabled = false;
			title.Annotations["REFRESH"] = "Refreshing";
			Text = title.Compose(true);
			if (mnuEnableAntiAntiDump.Checked)
				await RefreshModuleListAAD();
			else
				RefreshModuleList();
		}
		finally {
			title.Annotations["REFRESH"] = null;
			Text = title.Compose(true);
			mnuRefreshModuleList.Enabled = true;
			mnuOnlyDotNetModule.Enabled = true;
		}
	}

	void mnuViewFunctions_Click(object sender, EventArgs e) {
		if (!TryGetSelectedModule(out var module))
			return;

		new FunctionsForm(process.Id, module.ImageBase).Show();
	}

	void mnuOnlyDotNetModule_Click(object sender, EventArgs e) {
		mnuRefreshModuleList_Click(sender, e);
	}

	void mnuGotoLocation_Click(object sender, EventArgs e) {
		if (!TryGetSelectedModule(out var module))
			return;

		if (module.FilePath != "InMemory")
			Process.Start("explorer.exe", $"/select,{module.FilePath}");
	}

	async void mnuEnableAntiAntiDump_Click(object sender, EventArgs e) {
		if (mnuEnableAntiAntiDump.Checked == true) {
			mnuEnableAntiAntiDump.Checked = false;
			title.Annotations["ENABLE_AAD"] = null;
			mnuRefreshModuleList_Click(sender, e);
			return;
		}

		try {
			mnuEnableAntiAntiDump.Enabled = false;
			title.Annotations["ENABLE_AAD"] = "Enabling AntiAntiDump";
			Text = title.Compose(true);

			await Task.Run(() => AntiAntiDump.AADExtensions.EnumerateAADClients(process.Id));
			mnuEnableAntiAntiDump.Checked = true;
		}
		finally {
			title.Annotations["ENABLE_AAD"] = "AntiAntiDump";
			Text = title.Compose(true);
			mnuEnableAntiAntiDump.Enabled = true;
		}
		mnuRefreshModuleList_Click(sender, e);
	}
	#endregion

	bool TryGetSelectedModule([NotNullWhen(true)] out ModuleInfo? module) {
		module = null;
		if (lvwModules.SelectedIndices.Count == 0)
			return false;

		nuint moduleHandle = (nuint)ulong.Parse(lvwModules.GetFirstSelectedSubItem(chModuleHandle.Index).Text.Substring(2), NumberStyles.HexNumber);
		var domainName = lvwModules.GetFirstSelectedSubItem(chDomainName.Index).Text;
		if (string.IsNullOrEmpty(domainName))
			domainName = null;
		module = modules.Find(t => t.ImageBase == moduleHandle && (t as DotNetModuleInfo)?.DomainName == domainName);
		Debug2.Assert(module is not null);
		return true;
	}

	void RefreshModuleList() {
		Utils.RefreshListView(lvwModules, GetAllModules(), t => CreateListViewItem(t), -1);
	}

	ModuleInfo[] GetAllModules() {
		var modules = GetModules();
		var dnModules = GetDotNetModules().Where(t => !IsAntiAntiDumpModule(t)).ToArray();
		var allModules = dnModules.Concat(modules.Where(x => !dnModules.Any(y => x.ImageBase == y.ImageBase))).ToArray();
		this.modules.Clear();
		this.modules.AddRange(allModules);
		return allModules;
	}

	ModuleInfo[] GetModules() {
		if (mnuOnlyDotNetModule.Checked)
			return Array2.Empty<ModuleInfo>();

		return ModulesProviderFactory.Create(process.Id, ModulesProviderType.Unmanaged).EnumerateModules().ToArray();
	}

	ModuleInfo[] GetDotNetModules() {
		if (process is not DotNetProcessInfo)
			return Array2.Empty<ModuleInfo>();

		try {
			return ModulesProviderFactory.Create(process.Id, ModulesProviderType.Managed).EnumerateModules().ToArray();
		}
		catch (Exception ex) {
			Logger.Error("Fail to get .NET modules");
			Logger.Exception(ex);
			return Array2.Empty<ModuleInfo>();
		}
	}

	async Task RefreshModuleListAAD() {
		await Utils.RefreshListViewAsync(lvwModules, GetModulesAAD(), t => CreateListViewItem(t), -1);
	}

	IEnumerable<ModuleInfo> GetModulesAAD() {
		modules.Clear();
		var dnModules = new List<ModuleInfo>();
		foreach (var module in ModulesProviderFactory.Create(process.Id, ModulesProviderType.ManagedAAD).EnumerateModules()) {
			if (IsAntiAntiDumpModule(module))
				continue;
			dnModules.Add(module);
			yield return module;
		}
		modules.AddRange(dnModules);
		var umModules = GetModules().Where(x => !dnModules.Any(y => x.ImageBase == y.ImageBase)).ToArray();
		foreach (var module in umModules)
			yield return module;
		modules.AddRange(modules);
	}

	static ListViewItem CreateListViewItem(ModuleInfo module) {
		var listViewItem = new ListViewItem(string.IsNullOrEmpty(module.Name) ? "<<EmptyName>>" : module.Name);
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
		listViewItem.SubItems.Add(Formatter.FormatHex(module.ImageBase));
		// Address
		listViewItem.SubItems.Add(Formatter.FormatHex(module.ImageSize));
		// Size
		listViewItem.SubItems.Add(string.IsNullOrEmpty(module.FilePath) ? "InMemory" : module.FilePath);
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

	bool DumpModule(nuint moduleHandle, ImageLayout imageLayout, string filePath) {
		using var dumper = DumperFactory.Create(process.Id, GetDumperType());
		return dumper.DumpModule(moduleHandle, imageLayout, filePath);
	}

	DumperType GetDumperType() {
		if (mnuEnableAntiAntiDump.Checked)
			return DumperType.AntiAntiDump;
		return dumperType.Value;
	}

	static string PathInsertPostfix(string path, string postfix) {
		return Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + postfix + Path.GetExtension(path));
	}

	static bool IsAntiAntiDumpModule(ModuleInfo module) {
		if (string.IsNullOrEmpty(module.Name))
			return false;
		switch (Path.GetFileNameWithoutExtension(module.Name)) {
		case "00000000":
		case "00000001":
		case "00000002":
		case "00000003":
		case "00000004":
		case "00000100":
		case "00000101":
		case "00000102":
		case "00000103":
		case "00000104":
		case "00000200":
		case "00000201":
		case "00000202":
		case "00000203":
		case "00000204":
		case "00000300":
		case "00000301":
		case "00000302":
		case "00000303":
		case "00000304":
			return true;
		default:
			return module.Name == AntiAntiDump.AADCoreInjector.GetAADCoreModuleNameIfLoaded();
		}
	}
}
