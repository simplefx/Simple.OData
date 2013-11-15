using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    partial class FluentClient<T>
    {
        public new IEnumerable<T> FindEntries()
        {
            return RectifyColumnSelection(_client.FindEntries(_command.ToString()), _command.SelectedColumns)
                .Select(x => x.ToObject<T>());
        }

        public new IEnumerable<T> FindEntries(bool scalarResult)
        {
            return RectifyColumnSelection(_client.FindEntries(_command.ToString(), scalarResult), _command.SelectedColumns)
                .Select(x => x.ToObject<T>());
        }

        public new IEnumerable<T> FindEntries(out int totalCount)
        {
            return RectifyColumnSelection(_client.FindEntries(_command.WithInlineCount().ToString(), out totalCount), _command.SelectedColumns)
                .Select(x => x.ToObject<T>());
        }

        public new IEnumerable<T> FindEntries(bool scalarResult, out int totalCount)
        {
            return RectifyColumnSelection(_client.FindEntries(_command.WithInlineCount().ToString(), scalarResult, out totalCount), _command.SelectedColumns)
                .Select(x => x.ToObject<T>());
        }

        public T FindEntry()
        {
            return RectifyColumnSelection(_client.FindEntry(_command.ToString()), _command.SelectedColumns)
                .ToObject<T>();
        }

        public object FindScalar()
        {
            return _client.FindScalar(_command.ToString());
        }

        public T InsertEntry(bool resultRequired = true)
        {
            return _client.InsertEntry(_command.CollectionName, _command.EntryData, resultRequired)
                .ToObject<T>();
        }

        public int UpdateEntry()
        {
            if (_command.HasFilter)
                return UpdateEntries();
            else
                return _client.UpdateEntry(_command.CollectionName, _command.KeyValues, _command.EntryData);
        }

        public int UpdateEntries()
        {
            return _client.UpdateEntries(_command.CollectionName, _command.ToString(), _command.EntryData);
        }

        public int DeleteEntry()
        {
            if (_command.HasFilter)
                return DeleteEntries();
            else
                return _client.DeleteEntry(_command.CollectionName, _command.KeyValues);
        }

        public int DeleteEntries()
        {
            return _client.DeleteEntries(_command.CollectionName, _command.ToString());
        }

        public void LinkEntry<U>(U linkedEntryKey, string linkName = null)
        {
            _client.LinkEntry(_command.CollectionName, _command.KeyValues, linkName ?? typeof(U).Name, linkedEntryKey.ToDictionary());
        }

        public void LinkEntry<U>(Expression<Func<T, U>> expression, U linkedEntryKey)
        {
            LinkEntry(linkedEntryKey, ExtractColumnName(expression));
        }

        public void LinkEntry(ODataExpression expression, IDictionary<string, object> linkedEntryKey)
        {
            LinkEntry(linkedEntryKey, expression.ToString());
        }

        public new void LinkEntry(ODataExpression expression, ODataEntry linkedEntryKey)
        {
            LinkEntry(linkedEntryKey, expression.ToString());
        }

        public void UnlinkEntry<U>(string linkName = null)
        {
            _client.UnlinkEntry(_command.CollectionName, _command.KeyValues, linkName ?? typeof(U).Name);
        }

        public void UnlinkEntry<U>(Expression<Func<T, U>> expression)
        {
            UnlinkEntry(ExtractColumnName(expression));
        }

        public new void UnlinkEntry(ODataExpression expression)
        {
            _client.UnlinkEntry(_command.CollectionName, _command.KeyValues, expression.ToString());
        }

        public new IEnumerable<T> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            return RectifyColumnSelection(_client.ExecuteFunction(_command.ToString(), parameters), _command.SelectedColumns)
                .Select(x => x.ToObject<T>());
        }

        public new T ExecuteFunctionAsScalar(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsScalar<T>(_command.ToString(), parameters);
        }

        public new T[] ExecuteFunctionAsArray(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsArray<T>(_command.ToString(), parameters);
        }

        internal static IEnumerable<IDictionary<string, object>> RectifyColumnSelection(IEnumerable<IDictionary<string, object>> entries, IList<string> selectedColumns)
        {
            return entries.Select(x => RectifyColumnSelection(x, selectedColumns));
        }

        internal static IDictionary<string, object> RectifyColumnSelection(IDictionary<string, object> entry, IList<string> selectedColumns)
        {
            if (selectedColumns == null || !selectedColumns.Any())
            {
                return entry;
            }
            else
            {
                return entry.Where(x => selectedColumns.Any(y => x.Key.Homogenize() == y.Homogenize())).ToIDictionary();
            }
        }
    }
}
