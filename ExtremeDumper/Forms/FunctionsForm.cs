using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using FastWin32.Diagnostics;

namespace ExtremeDumper.Forms
{
    internal partial class FunctionsForm : Form
    {
        private uint _processId;

        private IntPtr _moduleHandle;

        private ResourceManager _resources = new ResourceManager(typeof(FunctionsForm));

        public FunctionsForm(uint processId, IntPtr moduleHandle, string moduleName)
        {
            InitializeComponent();
            _processId = processId;
            _moduleHandle = moduleHandle;
            Text = $"{_resources.GetString("StrExportFunctions")} {moduleName}(0x{moduleHandle.ToString(Cache.Is64BitOperatingSystem ? "X16" : "X8")})";
            typeof(ListView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, lvwFunctions, new object[] { true });
            lvwFunctions.ListViewItemSorter = new ListViewItemSorter(lvwFunctions, new Dictionary<int, TypeCode> { { 0, TypeCode.String }, { 1, Cache.Is64BitOperatingSystem ? TypeCode.UInt64 : TypeCode.UInt32 }, { 2, TypeCode.Int16 } }) { AllowHexLeadingSign = true };
            RefreshFunctionList();
        }

        #region Events
        private void lvwFunctions_Resize(object sender, EventArgs e) => lvwFunctions.AutoResizeColumns(true);

        private void mnuRefreshFunctionList_Click(object sender, EventArgs e) => RefreshFunctionList();

        private void mnuForceFalse_Click(object sender, EventArgs e) => throw new NotImplementedException();

        private void mnuForceTrue_Click(object sender, EventArgs e) => throw new NotImplementedException();

        private void mnuForceReturn_Click(object sender, EventArgs e) => throw new NotImplementedException();
        #endregion

        private void RefreshFunctionList()
        {
            ListViewItem listViewItem;

            lvwFunctions.Items.Clear();
            Module32.EnumFunctions(_processId, _moduleHandle, (IntPtr pFunction, string functionName, short ordinal) =>
            {
                listViewItem = new ListViewItem(functionName);
                listViewItem.SubItems.Add("0x" + pFunction.ToString(Cache.Is64BitOperatingSystem ? "X16" : "X8"));
                listViewItem.SubItems.Add(ordinal.ToString());
                lvwFunctions.Items.Add(listViewItem);
                return true;
            });
            lvwFunctions.AutoResizeColumns(false);
        }
    }
}
