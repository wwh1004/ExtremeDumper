using System;
using System.Windows.Forms;
using NativeSharp;

namespace ExtremeDumper.Forms;

unsafe partial class FunctionsForm : Form {
	readonly NativeProcess process;
	readonly NativeModule module;

	public FunctionsForm(uint processId, nuint moduleHandle) {
		InitializeComponent();
		Utils.ScaleByDpi(this);
		process = NativeProcess.Open(processId);
		if (process.IsInvalid)
			throw new InvalidOperationException();
		module = process.UnsafeGetModule((void*)moduleHandle);
		Text = TitleComposer.Compose(true, "Export Functions", module.Name, null);
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
			listViewItem.SubItems.Add(Formatter.FormatHex((nuint)functionInfo.Address));
			listViewItem.SubItems.Add(functionInfo.Ordinal.ToString());
			lvwFunctions.Items.Add(listViewItem);
		}
		lvwFunctions.AutoResizeColumns(false);
	}

	protected override void Dispose(bool disposing) {
		if (disposing) {
			components?.Dispose();
			process.Dispose();
		}
		base.Dispose(disposing);
	}
}
