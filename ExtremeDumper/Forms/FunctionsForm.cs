#nullable disable
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NativeSharp;

namespace ExtremeDumper.Forms {
	internal unsafe partial class FunctionsForm : Form {
		private readonly NativeModule _module;

		public FunctionsForm(NativeModule module) {
			if (module is null)
				throw new ArgumentNullException(nameof(module));

			InitializeComponent();
			_module = module;
			Text = $"Export Functions {_module.Name}({Utils.FormatPointer(_module.Handle)})";
			Utils.EnableDoubleBuffer(lvwFunctions);
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
				listViewItem.SubItems.Add(Utils.FormatPointer(functionInfo.Address));
				listViewItem.SubItems.Add(functionInfo.Ordinal.ToString());
				lvwFunctions.Items.Add(listViewItem);
			}
			lvwFunctions.AutoResizeColumns(false);
		}
	}
}
