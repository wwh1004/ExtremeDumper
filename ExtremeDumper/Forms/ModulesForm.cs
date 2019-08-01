using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using ExtremeDumper.Dumper;
using Microsoft.Diagnostics.Runtime;
using NativeSharp;
using static ExtremeDumper.Forms.NativeMethods;
using ImageLayout = dnlib.PE.ImageLayout;

namespace ExtremeDumper.Forms {
	internal unsafe partial class ModulesForm : Form {
		private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
		private readonly NativeProcess _process;
		private readonly bool _isDotNetProcess;
		private readonly DumperTypeWrapper _dumperType;
		private readonly ResourceManager _resources = new ResourceManager(typeof(ModulesForm));

		public ModulesForm(uint processId, string processName, bool isDotNetProcess, DumperTypeWrapper dumperType) {
			InitializeComponent();
			_process = NativeProcess.Open(processId);
			if (_process == NativeProcess.InvalidProcess)
				throw new InvalidOperationException();
			_isDotNetProcess = isDotNetProcess;
			_dumperType = dumperType;
			Text = $"{_resources.GetString("StrModules")} {processName}(ID={processId.ToString()})";
			typeof(ListView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, lvwModules, new object[] { true });
			lvwModules.ListViewItemSorter = new ListViewItemSorter(lvwModules, new List<TypeCode> {
				TypeCode.String,
				TypeCode.String,
				TypeCode.String,
				TypeCode.UInt64,
				TypeCode.Int32,
				TypeCode.String
			}) {
				AllowHexLeading = true
			};
			RefreshModuleList();
		}

		#region Events
		private void lvwModules_Resize(object sender, EventArgs e) {
			lvwModules.AutoResizeColumns(true);
		}

		private void mnuDumpModule_Click(object sender, EventArgs e) {
			if (lvwModules.SelectedIndices.Count == 0)
				return;

			string filePath;
			string directoryPath;
			IntPtr moduleHandle;

			filePath = PathInsertPostfix(EnsureValidFileName(lvwModules.GetFirstSelectedSubItem(chModuleName.Index).Text), ".dump");
			if (filePath.EndsWith(".dump", StringComparison.Ordinal))
				filePath += ".dll";
			sfdlgDumped.FileName = filePath;
			directoryPath = lvwModules.GetFirstSelectedSubItem(chModulePath.Index).Text;
			if (directoryPath != "InMemory" && Directory.Exists(directoryPath))
				sfdlgDumped.InitialDirectory = directoryPath;
			if (sfdlgDumped.ShowDialog() != DialogResult.OK)
				return;
			moduleHandle = (IntPtr)ulong.Parse(lvwModules.GetFirstSelectedSubItem(chModuleHandle.Index).Text.Substring(2), NumberStyles.HexNumber, null);
			DumpModule(moduleHandle, directoryPath == "InMemory" ? ImageLayout.File : ImageLayout.Memory, sfdlgDumped.FileName);
		}

		private void mnuRefreshModuleList_Click(object sender, EventArgs e) {
			RefreshModuleList();
		}

		private void mnuViewFunctions_Click(object sender, EventArgs e) {
			if (lvwModules.SelectedIndices.Count == 0)
				return;

			FunctionsForm functionsForm;

#pragma warning disable IDE0067
			functionsForm = new FunctionsForm(_process.UnsafeGetModule((void*)ulong.Parse(lvwModules.GetFirstSelectedSubItem(chModuleHandle.Index).Text.Substring(2), NumberStyles.HexNumber, null)));
#pragma warning restore IDE0067
			functionsForm.Show();
		}

		private void mnuOnlyDotNetModule_Click(object sender, EventArgs e) {
			RefreshModuleList();
		}

		private void mnuGotoLocation_Click(object sender, EventArgs e) {
			if (lvwModules.SelectedIndices.Count == 0)
				return;

			string filePath;

			filePath = lvwModules.GetFirstSelectedSubItem(chModulePath.Index).Text;
			if (filePath != "InMemory")
				Process.Start("explorer.exe", @"/select, " + filePath);
		}
		#endregion

		private void RefreshModuleList() {
			IntPtr snapshotHandle;
			MODULEENTRY32 moduleEntry32;
			ListViewItem listViewItem;
			DataTarget dataTarget;

			lvwModules.Items.Clear();
			if (!mnuOnlyDotNetModule.Checked) {
				moduleEntry32 = MODULEENTRY32.Default;
				snapshotHandle = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, _process.Id);
				if (snapshotHandle == INVALID_HANDLE_VALUE)
					return;
				if (!Module32First(snapshotHandle, ref moduleEntry32))
					return;
				do {
					listViewItem = new ListViewItem(moduleEntry32.szModule);
					// Name
					listViewItem.SubItems.Add(string.Empty);
					// Domain Name
					listViewItem.SubItems.Add(string.Empty);
					// CLR Version
					listViewItem.SubItems.Add("0x" + moduleEntry32.modBaseAddr.ToString(Cache.Is64BitProcess ? "X16" : "X8"));
					// BaseAddress
					listViewItem.SubItems.Add("0x" + moduleEntry32.modBaseSize.ToString("X8"));
					// Size
					listViewItem.SubItems.Add(moduleEntry32.szExePath);
					// Path
					lvwModules.Items.Add(listViewItem);
				} while (Module32Next(snapshotHandle, ref moduleEntry32));
			}
			if (_isDotNetProcess)
				try {
					using (dataTarget = DataTarget.AttachToProcess((int)_process.Id, 10000, AttachFlag.Passive))
						foreach (ClrInfo clrInfo in dataTarget.ClrVersions)
							foreach (ClrAppDomain clrAppDomain in clrInfo.CreateRuntime().AppDomains)
								foreach (ClrModule clrModule in clrAppDomain.Modules) {
									try {
										string moduleName;

										moduleName = clrModule.Name ?? "EmptyName";
										moduleName = clrModule.IsDynamic ? moduleName.Split(',')[0] : Path.GetFileName(moduleName);
										listViewItem = new ListViewItem(moduleName);
										// Name
										listViewItem.SubItems.Add(clrAppDomain.Name);
										// Domain Name
										listViewItem.SubItems.Add(clrInfo.Version.ToString());
										// CLR Version
										listViewItem.SubItems.Add("0x" + clrModule.ImageBase.ToString(Cache.Is64BitProcess ? "X16" : "X8"));
										// BaseAddress
										listViewItem.SubItems.Add("0x" + clrModule.Size.ToString("X8"));
										// Size
										listViewItem.SubItems.Add(clrModule.IsDynamic ? "InMemory" : clrModule.FileName);
										// Path
										listViewItem.BackColor = Cache.DotNetColor;
										lvwModules.Items.Add(listViewItem);
									}
									catch {
									}
								}
				}
				catch {
				}
			lvwModules.AutoResizeColumns(false);
		}

		private static string EnsureValidFileName(string fileName) {
			if (string.IsNullOrEmpty(fileName))
				return string.Empty;

			StringBuilder newFileName;

			newFileName = new StringBuilder(fileName.Length);
			foreach (char chr in fileName)
				if (!InvalidFileNameChars.Contains(chr))
					newFileName.Append(chr);
			return newFileName.ToString();
		}

		private void DumpModule(IntPtr moduleHandle, ImageLayout imageLayout, string filePath) {
			bool result;

			using (IDumper dumper = DumperFactory.GetDumper(_process.Id, _dumperType.Value))
				result = dumper.DumpModule(moduleHandle, imageLayout, filePath);
			MessageBoxStub.Show(result ? $"{_resources.GetString("StrDumpModuleSuccessfully")}{Environment.NewLine}{filePath}" : _resources.GetString("StrFailToDumpModule"), result ? MessageBoxIcon.Information : MessageBoxIcon.Error);
		}

		private static string PathInsertPostfix(string path, string postfix) {
			return Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + postfix + Path.GetExtension(path));
		}

		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
				_process.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
