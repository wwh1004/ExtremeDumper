using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using NativeSharp;

namespace ExtremeDumper.Forms {
	internal unsafe partial class FunctionsForm : Form {
		private readonly NativeModule _module;
		private readonly ResourceManager _resources = new ResourceManager(typeof(FunctionsForm));

		public FunctionsForm(NativeModule module) {
			if (module is null)
				throw new ArgumentNullException(nameof(module));

			InitializeComponent();
			_module = module;
			Text = $"{_resources.GetString("StrExportFunctions")} {_module.Name}(0x{((IntPtr)_module.Handle).ToString(Cache.Is64BitProcess ? "X16" : "X8")})";
			typeof(ListView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, lvwFunctions, new object[] { true });
			lvwFunctions.ListViewItemSorter = new ListViewItemSorter(lvwFunctions, new List<TypeCode> {
				TypeCode.String,
				TypeCode.UInt64,
				TypeCode.Int16
			}) {
				AllowHexLeading = true
			};
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
			foreach (var functionInfo in _module.EnumerateFunctionInfos()) {
				var listViewItem = new ListViewItem(functionInfo.Name);
				listViewItem.SubItems.Add("0x" + ((IntPtr)functionInfo.Address).ToString(Cache.Is64BitProcess ? "X16" : "X8"));
				listViewItem.SubItems.Add(functionInfo.Ordinal.ToString());
				lvwFunctions.Items.Add(listViewItem);
			}
			lvwFunctions.AutoResizeColumns(false);
		}
	}
}
