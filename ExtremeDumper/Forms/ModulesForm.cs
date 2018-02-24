using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using static ExtremeDumper.Forms.NativeMethods;

namespace ExtremeDumper.Forms
{
    internal partial class ModulesForm : Form
    {
        private uint _processId;

        private string _processName;

        private bool _isDotNetProcess;

        private DumperCoreWrapper _dumperCore;

        public ModulesForm(uint processId, string processName, bool isDotNetProcess, DumperCoreWrapper dumperCore)
        {
            InitializeComponent();
            _processId = processId;
            _processName = processName;
            _isDotNetProcess = isDotNetProcess;
            _dumperCore = dumperCore;
            Text = $"进程{processName}(ID={processId.ToString()})的模块列表";
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

            if (fbdlgDumped.ShowDialog() != DialogResult.OK)
                return;
            DumpModule((IntPtr)(Cache.Is64BitOperatingSystem ? ulong.Parse(lvwModules.SelectedItems[0].SubItems[1].Text.Substring(2), NumberStyles.HexNumber, null) : uint.Parse(lvwModules.SelectedItems[0].SubItems[1].Text.Substring(2), NumberStyles.HexNumber, null)), fbdlgDumped.SelectedPath);
        }

        private void mnuRefreshModuleList_Click(object sender, EventArgs e) => RefreshModuleList();

        private void mnuViewFunctions_Click(object sender, EventArgs e)
        {
            if (lvwModules.SelectedIndices.Count == 0)
                return;

            new FunctionsForm(_processId, _processName, (IntPtr)(Cache.Is64BitOperatingSystem ? ulong.Parse(lvwModules.SelectedItems[0].SubItems[1].Text.Substring(2), NumberStyles.HexNumber, null) : uint.Parse(lvwModules.SelectedItems[0].SubItems[1].Text.Substring(2), NumberStyles.HexNumber, null)), lvwModules.SelectedItems[0].Text).Show();
        }

        private void mnuOnlyDotNetModule_Click(object sender, EventArgs e)
        {
            if (lvwModules.SelectedIndices.Count == 0)
                return;

        }
        #endregion

        private void RefreshModuleList()
        {
            IntPtr snapshotHandle;
            MODULEENTRY32 moduleEntry32;
            ListViewItem listViewItem;

            lvwModules.Items.Clear();
            moduleEntry32 = MODULEENTRY32.Default;
            snapshotHandle = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, _processId);
            if (snapshotHandle == INVALID_HANDLE_VALUE)
                return;
            if (!Module32First(snapshotHandle, ref moduleEntry32))
                return;
            do
            {
                listViewItem = new ListViewItem(moduleEntry32.szModule);
                listViewItem.SubItems.Add("0x" + moduleEntry32.modBaseAddr.ToString(Cache.Is64BitOperatingSystem ? "X16" : "X8"));
                listViewItem.SubItems.Add("0x" + moduleEntry32.modBaseSize.ToString("X8"));
                listViewItem.SubItems.Add("0x" + moduleEntry32.hModule.ToString(Cache.Is64BitOperatingSystem ? "X16" : "X8"));
                listViewItem.SubItems.Add(moduleEntry32.szExePath);
                lvwModules.Items.Add(listViewItem);
            }
            while (Module32Next(snapshotHandle, ref moduleEntry32));
            lvwModules.AutoResizeColumns(false);
        }

        private void DumpModule(IntPtr moduleHandle, string path)
        {
            bool result;

            result = DumperFactory.GetDumper(_processId, _dumperCore.Value).DumpModule(moduleHandle, path);
            MessageBoxStub.Show(result ? $"成功！文件被转储在:{Environment.NewLine}{path}" : "失败！", result ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        private void DumpModule(uint moduleId, string path)
        {
            bool result;

            result = DumperFactory.GetDumper(_processId, _dumperCore.Value).DumpModule(moduleId, path);
            MessageBoxStub.Show(result ? $"成功！文件被转储在:{Environment.NewLine}{path}" : "失败！", result ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }
    }
}
