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
using Microsoft.Diagnostics.Runtime;
using static ExtremeDumper.Forms.NativeMethods;

namespace ExtremeDumper.Forms
{
    internal partial class ModulesForm : Form
    {
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        private uint _processId;

        private bool _isDotNetProcess;

        private DumperCoreWrapper _dumperCore;

        private ResourceManager _resources = new ResourceManager(typeof(ModulesForm));

        public ModulesForm(uint processId, string processName, bool isDotNetProcess, DumperCoreWrapper dumperCore)
        {
            InitializeComponent();
            _processId = processId;
            _isDotNetProcess = isDotNetProcess;
            _dumperCore = dumperCore;
            Text = $"{_resources.GetString("StrModules")} {processName}(ID={processId.ToString()})";
            mnuOnlyDotNetModule.Checked = isDotNetProcess;
            typeof(ListView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, lvwModules, new object[] { true });
            lvwModules.ListViewItemSorter = new ListViewItemSorter(lvwModules, new Dictionary<int, TypeCode> { { 0, TypeCode.String }, { 1, Cache.Is64BitOperatingSystem ? TypeCode.UInt64 : TypeCode.UInt32 }, { 2, TypeCode.Int32 }, { 3, Cache.Is64BitOperatingSystem ? TypeCode.UInt64 : TypeCode.UInt32 }, { 4, TypeCode.String } }) { AllowHexLeadingSign = true };
            RefreshModuleList();
        }

        #region Events
        private void lvwModules_Resize(object sender, EventArgs e) => lvwModules.AutoResizeColumns(true);

        private void mnuDumpModule_Click(object sender, EventArgs e)
        {
            if (lvwModules.SelectedIndices.Count == 0)
                return;

            IntPtr moduleHandle;

            if (fbdlgDumped.ShowDialog() != DialogResult.OK)
                return;
            moduleHandle = (IntPtr)(Cache.Is64BitOperatingSystem ? ulong.Parse(lvwModules.SelectedItems[0].SubItems[1].Text.Substring(2), NumberStyles.HexNumber, null) : uint.Parse(lvwModules.SelectedItems[0].SubItems[1].Text.Substring(2), NumberStyles.HexNumber, null));
            DumpModule(moduleHandle, Path.Combine(fbdlgDumped.SelectedPath, EnsureValidFileName(lvwModules.SelectedItems[0].Text)));
        }

        private void mnuRefreshModuleList_Click(object sender, EventArgs e) => RefreshModuleList();

        private void mnuViewFunctions_Click(object sender, EventArgs e)
        {
            if (lvwModules.SelectedIndices.Count == 0)
                return;

            new FunctionsForm(_processId, (IntPtr)(Cache.Is64BitOperatingSystem ? ulong.Parse(lvwModules.SelectedItems[0].SubItems[1].Text.Substring(2), NumberStyles.HexNumber, null) : uint.Parse(lvwModules.SelectedItems[0].SubItems[1].Text.Substring(2), NumberStyles.HexNumber, null)), lvwModules.SelectedItems[0].Text).Show();
        }

        private void mnuOnlyDotNetModule_Click(object sender, EventArgs e) => RefreshModuleList();

        private void mnuGotoLocation_Click(object sender, EventArgs e)
        {
            if (lvwModules.SelectedIndices.Count == 0)
                return;

            string filePath = lvwModules.SelectedItems[0].SubItems[3].Text;

            if (filePath != "InMemory")
                Process.Start("explorer.exe", @"/select, " + filePath);
        }
        #endregion

        private void RefreshModuleList()
        {
            IntPtr snapshotHandle;
            MODULEENTRY32 moduleEntry32;
            ListViewItem listViewItem;
            DataTarget dataTarget;

            lvwModules.Items.Clear();
            if (!mnuOnlyDotNetModule.Checked)
            {
                moduleEntry32 = MODULEENTRY32.Default;
                snapshotHandle = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, _processId);
                if (snapshotHandle == INVALID_HANDLE_VALUE)
                    return;
                if (!Module32First(snapshotHandle, ref moduleEntry32))
                    return;
                if (!_isDotNetProcess)
                {
                    //如果是.Net进程,这里获取的主模块信息会与通过ClrMD获取的信息重复
                    listViewItem = new ListViewItem(moduleEntry32.szModule);
                    listViewItem.SubItems.Add("0x" + moduleEntry32.modBaseAddr.ToString(Cache.Is64BitOperatingSystem ? "X16" : "X8"));
                    listViewItem.SubItems.Add("0x" + moduleEntry32.modBaseSize.ToString("X8"));
                    listViewItem.SubItems.Add(moduleEntry32.szExePath);
                    lvwModules.Items.Add(listViewItem);
                }
                while (Module32Next(snapshotHandle, ref moduleEntry32))
                {
                    listViewItem = new ListViewItem(moduleEntry32.szModule);
                    listViewItem.SubItems.Add("0x" + moduleEntry32.modBaseAddr.ToString(Cache.Is64BitOperatingSystem ? "X16" : "X8"));
                    listViewItem.SubItems.Add("0x" + moduleEntry32.modBaseSize.ToString("X8"));
                    listViewItem.SubItems.Add(moduleEntry32.szExePath);
                    lvwModules.Items.Add(listViewItem);
                }
            }
            if (_isDotNetProcess)
                using (dataTarget = DataTarget.AttachToProcess((int)_processId, 10000, AttachFlag.Passive))
                    foreach (ClrInfo clrInfo in dataTarget.ClrVersions)
                        foreach (ClrModule clrModule in clrInfo.CreateRuntime().Modules)
                        {
                            string moduleName;

                            moduleName = clrModule.Name ?? "EmptyName";
                            moduleName = clrModule.IsDynamic ? moduleName.Split(',')[0] : Path.GetFileName(moduleName);
                            listViewItem = new ListViewItem(moduleName);
                            listViewItem.SubItems.Add("0x" + clrModule.ImageBase.ToString(Cache.Is64BitOperatingSystem ? "X16" : "X8"));
                            listViewItem.SubItems.Add("0x" + clrModule.Size.ToString("X8"));
                            listViewItem.SubItems.Add(clrModule.IsDynamic ? "InMemory" : clrModule.FileName);
                            listViewItem.BackColor = Cache.DotNetColor;
                            lvwModules.Items.Add(listViewItem);
                        }
            lvwModules.AutoResizeColumns(false);
        }

        private static string EnsureValidFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            StringBuilder newFileName;

            newFileName = new StringBuilder(fileName.Length);
            foreach (char chr in fileName)
                if (!InvalidFileNameChars.Contains(chr))
                    newFileName.Append(chr);
            return newFileName.ToString();
        }

        private void DumpModule(IntPtr moduleHandle, string filePath)
        {
            bool result;

            result = DumperFactory.GetDumper(_processId, _dumperCore.Value).DumpModule(moduleHandle, filePath);
            MessageBoxStub.Show(result ? $"{_resources.GetString("StrDumpModuleSuccessfully")}{Environment.NewLine}{filePath}" : _resources.GetString("StrFailToDumpModule"), result ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }
    }
}
