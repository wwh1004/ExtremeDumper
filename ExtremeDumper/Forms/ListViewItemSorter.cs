using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace ExtremeDumper.Forms
{
    internal class ListViewItemSorter : IComparer, IDisposable
    {
        private delegate bool Parser<T>(string s, NumberStyles style, IFormatProvider provider, out T result);

        private ListView _listView;

        private Dictionary<int, TypeCode> _columnMapping;

        private int _column;

        private int _lastColumn;

        private bool _isDisposed;

        public bool AllowHexLeadingSign { get; set; }

        public ListViewItemSorter(ListView listView, Dictionary<int, TypeCode> columnMapping)
        {
            _listView = listView ?? throw new ArgumentNullException();
            _columnMapping = columnMapping ?? throw new ArgumentNullException();
            listView.ColumnClick += ListView_ColumnClick;
        }

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (_listView.Sorting == SortOrder.None)
                return;

            _column = e.Column;
#pragma warning disable IDE0045
            if (_column == _lastColumn)
                _listView.Sorting = _listView.Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            else
                _listView.Sorting = SortOrder.Ascending;
#pragma warning restore IDE0045
            _listView.Sort();
            _lastColumn = _column;
        }

        public int Compare(object x, object y)
        {
            switch (_listView.Sorting)
            {
                case SortOrder.Ascending:
                    return Compare(((ListViewItem)x).SubItems[_column].Text, ((ListViewItem)y).SubItems[_column].Text);
                case SortOrder.Descending:
                    return -Compare(((ListViewItem)x).SubItems[_column].Text, ((ListViewItem)y).SubItems[_column].Text);
                default:
                    throw new InvalidEnumArgumentException("无效的Sorting");
            }
        }

        private int Compare(string x, string y)
        {
            switch (_columnMapping[_column])
            {
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

        private int IntegerComparer<T>(string x, string y, Parser<T> parser) where T : IComparable<T>
        {
            T xParsed;
            T yParsed;

            if (parser(x, NumberStyles.Integer, null, out xParsed) && parser(y, NumberStyles.Integer, null, out yParsed))
                return xParsed.CompareTo(yParsed);
            else if (parser(AllowHexLeadingSign ? x.Replace("0x", string.Empty) : x, NumberStyles.HexNumber, null, out xParsed) && parser(AllowHexLeadingSign ? y.Replace("0x", string.Empty) : y, NumberStyles.HexNumber, null, out yParsed))
                return xParsed.CompareTo(yParsed);
            else
                throw new ArgumentException();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;
            _listView.ColumnClick -= ListView_ColumnClick;
            _columnMapping = null;
            _isDisposed = true;
        }
    }
}
