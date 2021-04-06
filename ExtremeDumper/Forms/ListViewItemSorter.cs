using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace ExtremeDumper.Forms {
	internal class ListViewItemSorter : IComparer, IDisposable {
		private delegate bool Parser<T>(string s, NumberStyles style, IFormatProvider provider, out T result);

		private readonly ListView _listView;

		private List<TypeCode> _columnTypes;

		private int _column;

		private int _lastColumn;

		private bool _isDisposed;

		public bool AllowHexLeading { get; set; }

		public ListViewItemSorter(ListView listView, List<TypeCode> columnTypes) {
			if (listView is null)
				throw new ArgumentNullException(nameof(listView));
			if (columnTypes is null)
				throw new ArgumentNullException(nameof(columnTypes));

			_listView = listView;
			_columnTypes = columnTypes;
			listView.ColumnClick += ListView_ColumnClick;
		}

		private void ListView_ColumnClick(object sender, ColumnClickEventArgs e) {
			if (_listView.Sorting == SortOrder.None)
				return;

			_column = e.Column;
			if (_column == _lastColumn)
				_listView.Sorting = _listView.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
			else
				_listView.Sorting = SortOrder.Ascending;
			_listView.Sort();
			_lastColumn = _column;
		}

		public int Compare(object x, object y) {
			switch (_listView.Sorting) {
			case SortOrder.Ascending:
				return Compare(((ListViewItem)x).SubItems[_column].Text, ((ListViewItem)y).SubItems[_column].Text);
			case SortOrder.Descending:
				return -Compare(((ListViewItem)x).SubItems[_column].Text, ((ListViewItem)y).SubItems[_column].Text);
			default:
				throw new InvalidEnumArgumentException("无效的Sorting");
			}
		}

		private int Compare(string x, string y) {
			switch (_columnTypes[_column]) {
			case TypeCode.Boolean:
				throw new NotImplementedException();
			case TypeCode.Char:
				throw new NotImplementedException();
			case TypeCode.SByte:
				throw new NotImplementedException();
			case TypeCode.Byte:
				throw new NotImplementedException();
			case TypeCode.Int16:
				return IntegerComparer<short>(x, y, short.TryParse);
			case TypeCode.UInt16:
				return IntegerComparer<ushort>(x, y, ushort.TryParse);
			case TypeCode.Int32:
				return IntegerComparer<int>(x, y, int.TryParse);
			case TypeCode.UInt32:
				return IntegerComparer<uint>(x, y, uint.TryParse);
			case TypeCode.Int64:
				return IntegerComparer<long>(x, y, long.TryParse);
			case TypeCode.UInt64:
				return IntegerComparer<ulong>(x, y, ulong.TryParse);
			case TypeCode.Single:
				throw new NotImplementedException();
			case TypeCode.Double:
				throw new NotImplementedException();
			case TypeCode.Decimal:
				throw new NotImplementedException();
			case TypeCode.DateTime:
				throw new NotImplementedException();
			case TypeCode.String:
				return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
			default:
				throw new InvalidEnumArgumentException();
			}
		}

		private int IntegerComparer<T>(string x, string y, Parser<T> parser) where T : IComparable<T> {
			if (parser(x, NumberStyles.Integer, null, out var xParsed) && parser(y, NumberStyles.Integer, null, out var yParsed))
				return xParsed.CompareTo(yParsed);
			else if (parser(AllowHexLeading ? CleanHexLeading(x) : x, NumberStyles.HexNumber, null, out xParsed) && parser(AllowHexLeading ? CleanHexLeading(y) : y, NumberStyles.HexNumber, null, out yParsed))
				return xParsed.CompareTo(yParsed);
			else
				throw new ArgumentException();
		}

		private static string CleanHexLeading(string value) {
			value = value.TrimStart();
			if (value.StartsWith("0X", StringComparison.OrdinalIgnoreCase))
				return value.Substring(2);
			else
				return value;
		}

		public void Dispose() {
			if (_isDisposed)
				return;

			_listView.ColumnClick -= ListView_ColumnClick;
			_columnTypes = null;
			_isDisposed = true;
		}
	}
}
