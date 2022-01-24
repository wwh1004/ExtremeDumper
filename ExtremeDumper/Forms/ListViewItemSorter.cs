using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace ExtremeDumper.Forms;

sealed class ListViewItemSorter : IComparer, IDisposable {
	delegate bool Parser<T>(string s, NumberStyles style, IFormatProvider? provider, out T result);

	readonly ListView listView;
	readonly IList<TypeCode> columnTypes;
	readonly Dictionary<string, object> parsedValues = new(StringComparer.Ordinal);
	int column;
	int lastColumn;
	bool isDisposed;

	public bool AllowHexLeading { get; set; }

	public ListViewItemSorter(ListView listView, IList<TypeCode> columnTypes) {
		if (listView is null)
			throw new ArgumentNullException(nameof(listView));
		if (columnTypes is null)
			throw new ArgumentNullException(nameof(columnTypes));

		this.listView = listView;
		this.columnTypes = columnTypes;
		listView.ColumnClick += ListView_ColumnClick;
	}

	void ListView_ColumnClick(object sender, ColumnClickEventArgs e) {
		if (listView.Sorting == SortOrder.None)
			return;

		column = e.Column;
		if (column == lastColumn)
			listView.Sorting = listView.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
		else
			listView.Sorting = SortOrder.Ascending;
		listView.Sort();
		lastColumn = column;
	}

	public int Compare(object x, object y) {
		switch (listView.Sorting) {
		case SortOrder.Ascending:
			return Compare(((ListViewItem)x).SubItems[column].Text, ((ListViewItem)y).SubItems[column].Text);
		case SortOrder.Descending:
			return -Compare(((ListViewItem)x).SubItems[column].Text, ((ListViewItem)y).SubItems[column].Text);
		default:
			throw new InvalidEnumArgumentException("无效的Sorting");
		}
	}

	int Compare(string x, string y) {
		switch (columnTypes[column]) {
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

	int IntegerComparer<T>(string x, string y, Parser<T> parser) where T : struct, IComparable<T> {
		var a = ParseInteger(x, parser);
		var b = ParseInteger(y, parser);
		return a.CompareTo(b);
	}

	T ParseInteger<T>(string value, Parser<T> parser) where T : struct {
		if (!parsedValues.TryGetValue(value, out var integer)) {
			bool isHex = false;
			value = value.Trim();
			if (AllowHexLeading)
				value = CleanHexIdentifier(value, out isHex);
			if (!parser(value, isHex ? NumberStyles.HexNumber : NumberStyles.Integer, null, out var t))
				t = default;
			integer = t;
			parsedValues.Add(value, integer);
		}
		return (T)integer;
	}

	static string CleanHexIdentifier(string value, out bool isHex) {
		string t = value.Trim();
		if (t.StartsWith("0X", StringComparison.OrdinalIgnoreCase)) {
			isHex = true;
			return t.Substring(2);
		}
		if (t.EndsWith("H", StringComparison.OrdinalIgnoreCase)) {
			isHex = true;
			return t.Substring(0, t.Length - 1);
		}
		isHex = false;
		return value;
	}

	public void Dispose() {
		if (isDisposed)
			return;

		listView.ColumnClick -= ListView_ColumnClick;
		columnTypes.Clear();
		isDisposed = true;
	}
}
