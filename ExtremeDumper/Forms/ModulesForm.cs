using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Diagnostics.Runtime;
using static ExtremeDumper.Forms.NativeMethods;

namespace ExtremeDumper.Forms
{
    internal partial class ModulesForm : Form
    {
        private uint _processId;

        private bool _isDotNetProcess;

        private DumperCoreWrapper _dumperCore;

        public ModulesForm(uint processId, string processName, bool isDotNetProcess, DumperCoreWrapper dumperCore)
        {
            InitializeComponent();
            _processId = processId;
            _isDotNetProcess = isDotNetProcess;
            _dumperCore = dumperCore;
            Text = $"进程{processName}(ID={processId.ToString()})的模块列表";
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

            if (fbdlgDumped.ShowDialog() != DialogResult.OK)
                return;
            DumpModule((IntPtr)(Cache.Is64BitOperatingSystem ? ulong.Parse(lvwModules.SelectedItems[0].SubItems[1].Text.Substring(2), NumberStyles.HexNumber, null) : uint.Parse(lvwModules.SelectedItems[0].SubItems[1].Text.Substring(2), NumberStyles.HexNumber, null)), Path.Combine(fbdlgDumped.SelectedPath, lvwModules.SelectedItems[0].Text));
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

            if (filePath == "模块仅在内存中")
                MessageBoxStub.Show("模块仅在内存中,可以在转储之后查看", MessageBoxIcon.Error);
            else
            {
                if (!Environment.Is64BitProcess && Cache.Is64BitOperatingSystem)
                    MessageBoxStub.Show("文件位置被重定向,资源管理器中显示的不一定是真实位置", MessageBoxIcon.Information);
                Process.Start("explorer.exe", @"/select, " + filePath);
            }
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
                            listViewItem = new ListViewItem(clrModule.IsDynamic ? clrModule.Name.Split(',')[0] : Path.GetFileName(clrModule.Name));
                            listViewItem.SubItems.Add("0x" + clrModule.ImageBase.ToString(Cache.Is64BitOperatingSystem ? "X16" : "X8"));
                            listViewItem.SubItems.Add("0x" + clrModule.Size.ToString("X8"));
                            listViewItem.SubItems.Add(clrModule.IsDynamic ? "模块仅在内存中" : clrModule.FileName);
                            listViewItem.BackColor = Cache.DotNetColor;
                            lvwModules.Items.Add(listViewItem);
                        }
            lvwModules.AutoResizeColumns(false);
        }

        private void DumpModule(IntPtr moduleHandle, string filePath)
        {
            bool result;

            result = DumperFactory.GetDumper(_processId, _dumperCore.Value).DumpModule(moduleHandle, filePath);
            MessageBoxStub.Show(result ? $"成功！文件被转储在:{Environment.NewLine}{filePath}" : "失败！", result ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }
    }
}
