using System;
using System.Drawing;
using System.Windows.Forms;
using static ExtremeDumper.Forms.NativeMethods;

namespace ExtremeDumper.Forms {
	internal static class ListViewExtesion {
		public static void AutoResizeColumns(this ListView listView, bool onlyLastColumn) {
			if (listView is null)
				throw new ArgumentNullException(nameof(listView));

			var scrollBarInfo = SCROLLBARINFO.Default;
			GetScrollBarInfo(listView.Handle, OBJID_VSCROLL, ref scrollBarInfo);
			int sumWidths = scrollBarInfo.dxyLineButton;
			if (onlyLastColumn) {
				foreach (ColumnHeader columnHeader in listView.Columns)
					sumWidths += columnHeader.Width;
				listView.Columns[listView.Columns.Count - 1].Width += listView.Width - sumWidths - 4;
			}
			else {
				int[] minWidths = new int[listView.Columns.Count];
				using (var g = listView.CreateGraphics()) {
					for (int i = 0; i < minWidths.Length; i++)
						minWidths[i] = (int)g.MeasureString(listView.Columns[i].Text, listView.Font).Width + 10;
				}
				listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
				for (int i = 0; i < minWidths.Length; i++) {
					if (listView.Columns[i].Width < minWidths[i])
						listView.Columns[i].Width = minWidths[i];
					sumWidths += listView.Columns[i].Width;
				}
				listView.Columns[minWidths.Length - 1].Width += listView.Width - sumWidths;
			}
		}

		public static ListViewItem.ListViewSubItem GetFirstSelectedSubItem(this ListView listView, int index) {
			if (listView is null)
				throw new ArgumentNullException(nameof(listView));

			return listView.SelectedItems[0].SubItems[index];
		}
	}
}
