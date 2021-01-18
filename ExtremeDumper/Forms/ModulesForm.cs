using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExtremeDumper.Dumping;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Utilities;
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
			Text = $"{_resources.GetString("StrModules")} {processName}(ID={processId})";
			typeof(ListView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, lvwModules, new object[] { true });
			lvwModules.ListViewItemSorter = new ListViewItemSorter(lvwModules, new List<TypeCode> { TypeCode.String, TypeCode.String, TypeCode.String, TypeCode.UInt64, TypeCode.Int32, TypeCode.String }) {
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

			string filePath = EnsureValidFileName(lvwModules.GetFirstSelectedSubItem(chModuleName.Index).Text);
			if (filePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && filePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
				filePath = PathInsertPostfix(filePath, ".dump");
			else
				filePath += ".dump.dll";
			sfdlgDumped.FileName = filePath;
			sfdlgDumped.InitialDirectory = Path.GetDirectoryName(_process.GetMainModule().ImagePath);
			if (sfdlgDumped.ShowDialog() != DialogResult.OK)
				return;
			var moduleHandle = (IntPtr)ulong.Parse(lvwModules.GetFirstSelectedSubItem(chModuleHandle.Index).Text.Substring(2), NumberStyles.HexNumber, null);
			DumpModule(moduleHandle, lvwModules.GetFirstSelectedSubItem(chModulePath.Index).Text == "InMemory" ? ImageLayout.File : ImageLayout.Memory, sfdlgDumped.FileName);
		}

		private void mnuRefreshModuleList_Click(object sender, EventArgs e) {
			RefreshModuleList();
		}

		private void mnuViewFunctions_Click(object sender, EventArgs e) {
			if (lvwModules.SelectedIndices.Count == 0)
				return;

			var functionsForm = new FunctionsForm(_process.UnsafeGetModule((void*)ulong.Parse(lvwModules.GetFirstSelectedSubItem(chModuleHandle.Index).Text.Substring(2), NumberStyles.HexNumber, null)));
			functionsForm.Show();
		}

		private void mnuOnlyDotNetModule_Click(object sender, EventArgs e) {
			RefreshModuleList();
		}

		private void mnuGotoLocation_Click(object sender, EventArgs e) {
			if (lvwModules.SelectedIndices.Count == 0)
				return;

			string filePath = lvwModules.GetFirstSelectedSubItem(chModulePath.Index).Text;
			if (filePath != "InMemory")
				Process.Start("explorer.exe", @"/select, " + filePath);
		}
		#endregion

		private void RefreshModuleList() {
			lvwModules.Items.Clear();
			ListViewItem listViewItem;
			if (!mnuOnlyDotNetModule.Checked) {
				var moduleEntry32 = MODULEENTRY32.Default;
				var snapshotHandle = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, _process.Id);
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
			if (_isDotNetProcess) {
				try {
					using (var dataTarget = DataTarget.AttachToProcess((int)_process.Id, 1000, AttachFlag.Passive)) {
						dataTarget.SymbolLocator = DummySymbolLocator.Instance;
						foreach (var clrModule in dataTarget.ClrVersions.Select(t => t.CreateRuntime()).SelectMany(t => t.AppDomains).SelectMany(t => t.Modules)) {
							string name = clrModule.Name;
							bool inMemory;
							if (!string.IsNullOrEmpty(name)) {
								inMemory = name.Contains(",");
							}
							else {
								name = "EmptyName";
								inMemory = true;
							}
							string moduleName = !inMemory ? Path.GetFileName(name) : name.Split(',')[0];
							listViewItem = new ListViewItem(moduleName);
							// Name
							listViewItem.SubItems.Add(string.Join(", ", clrModule.AppDomains.Select(t => t.Name)));
							// Domain Name
							listViewItem.SubItems.Add(clrModule.Runtime.ClrInfo.Version.ToString());
							// CLR Version
							listViewItem.SubItems.Add("0x" + clrModule.ImageBase.ToString(Cache.Is64BitProcess ? "X16" : "X8"));
							// BaseAddress
							listViewItem.SubItems.Add("0x" + clrModule.Size.ToString("X8"));
							// Size
							listViewItem.SubItems.Add(!inMemory ? name : "InMemory");
							// Path
							listViewItem.BackColor = Cache.DotNetColor;
							lvwModules.Items.Add(listViewItem);
						}
					}
				}
				catch {
					MessageBoxStub.Show(_resources.GetString("StrFailToGetDotNetModules"), MessageBoxIcon.Error);
				}
			}
			lvwModules.AutoResizeColumns(false);
		}

		private static string EnsureValidFileName(string fileName) {
			if (string.IsNullOrEmpty(fileName))
				return string.Empty;

			var newFileName = new StringBuilder(fileName.Length);
			foreach (char chr in fileName) {
				if (!InvalidFileNameChars.Contains(chr))
					newFileName.Append(chr);
			}
			return newFileName.ToString();
		}

		private void DumpModule(IntPtr moduleHandle, ImageLayout imageLayout, string filePath) {
			bool result;
			using (var dumper = DumperFactory.GetDumper(_process.Id, _dumperType.Value))
				result = dumper.DumpModule(moduleHandle, imageLayout, filePath);
			MessageBoxStub.Show(result ? $"{_resources.GetString("StrDumpModuleSuccessfully")}{Environment.NewLine}{filePath}" : _resources.GetString("StrFailToDumpModule"), result ? MessageBoxIcon.Information : MessageBoxIcon.Error);
		}

		private static string PathInsertPostfix(string path, string postfix) {
			return Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + postfix + Path.GetExtension(path));
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				components?.Dispose();
				_process.Dispose();
			}
			base.Dispose(disposing);
		}

		private sealed class DummySymbolLocator : SymbolLocator {
			public static DummySymbolLocator Instance { get; } = new DummySymbolLocator();

			private DummySymbolLocator() {
			}

			public override string FindBinary(string fileName, int buildTimeStamp, int imageSize, bool checkProperties = true) {
				return string.Empty;
			}

			public override Task<string> FindBinaryAsync(string fileName, int buildTimeStamp, int imageSize, bool checkProperties = true) {
				return Task.FromResult(string.Empty);
			}

			public override string FindPdb(string pdbName, Guid pdbIndexGuid, int pdbIndexAge) {
				return string.Empty;
			}

			public override Task<string> FindPdbAsync(string pdbName, Guid pdbIndexGuid, int pdbIndexAge) {
				return Task.FromResult(string.Empty);
			}

			protected override Task CopyStreamToFileAsync(Stream input, string fullSrcPath, string fullDestPath, long size) {
				throw new NotImplementedException();
			}
		}
	}
}
