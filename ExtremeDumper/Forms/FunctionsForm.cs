using System;
using System.Windows.Forms;
using NativeSharp;

namespace ExtremeDumper.Forms;

unsafe partial class FunctionsForm : Form {
	readonly NativeModule module;

	public FunctionsForm(NativeModule module) {
		if (module is null)
			throw new ArgumentNullException(nameof(module));

		InitializeComponent();
		this.module = module;
		Text = $"Export Functions {this.module.Name}({Utils.FormatPointer(this.module.Handle)})";
		Text = Utils.ObfuscateTitle(Text);
		Utils.EnableDoubleBuffer(lvwFunctions);
		lvwFunctions.ListViewItemSorter = new ListViewItemSorter(lvwFunctions, new[] { TypeCode.String, TypeCode.UInt64, TypeCode.Int16 }) { AllowHexLeading = true };
		RefreshFunctionList();
	}

	#region Events
	void lvwFunctions_Resize(object sender, EventArgs e) {
		lvwFunctions.AutoResizeColumns(true);
	}

	void mnuRefreshFunctionList_Click(object sender, EventArgs e) {
		RefreshFunctionList();
	}
	#endregion

	void RefreshFunctionList() {
		lvwFunctions.Items.Clear();
		foreach (var functionInfo in module.EnumerateFunctionInfos()) {
			var listViewItem = new ListViewItem(functionInfo.Name);
			listViewItem.SubItems.Add(Utils.FormatPointer(functionInfo.Address));
			listViewItem.SubItems.Add(functionInfo.Ordinal.ToString());
			lvwFunctions.Items.Add(listViewItem);
		}
		lvwFunctions.AutoResizeColumns(false);
	}
}
