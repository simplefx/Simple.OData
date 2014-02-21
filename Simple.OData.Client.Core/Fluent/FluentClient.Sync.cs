using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    partial class FluentClient<T>
    {
        public IEnumerable<T> FindEntries()
        {
            return RectifyColumnSelection((_client as ODataClient).FindEntries(_command), _command.SelectedColumns)
                .Select(x => x.ToObject<T>(_dynamicResults));
        }

        public IEnumerable<T> FindEntries(bool scalarResult)
        {
            return RectifyColumnSelection((_client as ODataClient).FindEntries(_command, scalarResult), _command.SelectedColumns)
                .Select(x => x.ToObject<T>(_dynamicResults));
        }

        public IEnumerable<T> FindEntries(out int totalCount)
        {
            return RectifyColumnSelection((_client as ODataClient).FindEntries(_command.WithInlineCount(), out totalCount), _command.SelectedColumns)
                .Select(x => x.ToObject<T>(_dynamicResults));
        }

        public IEnumerable<T> FindEntries(bool scalarResult, out int totalCount)
        {
            return RectifyColumnSelection((_client as ODataClient).FindEntries(_command.WithInlineCount(), scalarResult, out totalCount), _command.SelectedColumns)
                .Select(x => x.ToObject<T>(_dynamicResults));
        }

        public T FindEntry()
        {
            return RectifyColumnSelection((_client as ODataClient).FindEntry(_command), _command.SelectedColumns)
                .ToObject<T>(_dynamicResults);
        }

        public object FindScalar()
        {
            return (_client as ODataClient).FindScalar(_command);
        }

        public T InsertEntry(bool resultRequired = true)
        {
            return (_client as ODataClient).InsertEntry(_command, _command.EntryData, resultRequired)
                .ToObject<T>(_dynamicResults);
        }

        public int UpdateEntry()
        {
            if (_command.HasFilter)
                return UpdateEntries();
            else
                return (_client as ODataClient).UpdateEntry(_command, _command.KeyValues, _command.EntryData);
        }

        public int UpdateEntries()
        {
            return (_client as ODataClient).UpdateEntries(_command, _command.EntryData);
        }

        public int DeleteEntry()
        {
            if (_command.HasFilter)
                return DeleteEntries();
            else
                return (_client as ODataClient).DeleteEntry(_command, _command.KeyValues);
        }

        public int DeleteEntries()
        {
            return (_client as ODataClient).DeleteEntries(_command);
        }

        public void LinkEntry<U>(U linkedEntryKey, string linkName = null)
        {
            (_client as ODataClient).LinkEntry(_command, _command.KeyValues, linkName ?? typeof(U).Name, linkedEntryKey.ToDictionary());
        }

        public void LinkEntry<U>(Expression<Func<T, U>> expression, U linkedEntryKey)
        {
            LinkEntry(linkedEntryKey, ExtractColumnName(expression));
        }

        public void LinkEntry(ODataExpression expression, IDictionary<string, object> linkedEntryKey)
        {
            LinkEntry(linkedEntryKey, expression.ConvertToText());
        }

        public void LinkEntry(ODataExpression expression, ODataEntry linkedEntryKey)
        {
            LinkEntry(linkedEntryKey, expression.ConvertToText());
        }

        public void UnlinkEntry<U>(string linkName = null)
        {
            (_client as ODataClient).UnlinkEntry(_command, _command.KeyValues, linkName ?? typeof(U).Name);
        }

        public void UnlinkEntry<U>(Expression<Func<T, U>> expression)
        {
            UnlinkEntry(ExtractColumnName(expression));
        }

        public void UnlinkEntry(ODataExpression expression)
        {
            (_client as ODataClient).UnlinkEntry(_command, _command.KeyValues, expression.ConvertToText());
        }

        public IEnumerable<T> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            return RectifyColumnSelection((_client as ODataClient).ExecuteFunction(_command, parameters), _command.SelectedColumns)
                .Select(x => x.ToObject<T>(_dynamicResults));
        }

        public T ExecuteFunctionAsScalar(string functionName, IDictionary<string, object> parameters)
        {
            return (_client as ODataClient).ExecuteFunctionAsScalar<T>(_command, parameters);
        }

        public T[] ExecuteFunctionAsArray(string functionName, IDictionary<string, object> parameters)
        {
            return (_client as ODataClient).ExecuteFunctionAsArray<T>(_command, parameters);
        }

        public string GetCommandText()
        {
            return GetCommandTextAsync().Result;
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
