using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using NativeSharp;

namespace ExtremeDumper.Forms {
	internal partial class FunctionsForm : Form {
		private readonly NativeModule _module;
		private readonly ResourceManager _resources = new ResourceManager(typeof(FunctionsForm));

		public FunctionsForm(NativeModule module) {
			InitializeComponent();
			_module = module;
			Text = $"{_resources.GetString("StrExportFunctions")} {_module.Name}(0x{_module.Handle.ToString(Cache.Is64BitProcess ? "X16" : "X8")})";
			typeof(ListView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, lvwFunctions, new object[] { true });
			lvwFunctions.ListViewItemSorter = new ListViewItemSorter(lvwFunctions, new Dictionary<int, TypeCode> { { 0, TypeCode.String }, { 1, Cache.Is64BitProcess ? TypeCode.UInt64 : TypeCode.UInt32 }, { 2, TypeCode.Int16 } }) { AllowHexLeading = true };
			RefreshFunctionList();
		}

		#region Events
		private void lvwFunctions_Resize(object sender, EventArgs e) {
			lvwFunctions.AutoResizeColumns(true);
		}

		private void mnuRefreshFunctionList_Click(object sender, EventArgs e) {
			RefreshFunctionList();
		}
		#endregion

		private void RefreshFunctionList() {
			lvwFunctions.Items.Clear();
			foreach (ExportFunctionInfo functionInfo in _module.GetFunctionInfos()) {
				ListViewItem listViewItem;

				listViewItem = new ListViewItem(functionInfo.Name);
				listViewItem.SubItems.Add("0x" + functionInfo.Address.ToString(Cache.Is64BitProcess ? "X16" : "X8"));
				listViewItem.SubItems.Add(functionInfo.Ordinal.ToString());
				lvwFunctions.Items.Add(listViewItem);
			}
			lvwFunctions.AutoResizeColumns(false);
		}
	}
}
