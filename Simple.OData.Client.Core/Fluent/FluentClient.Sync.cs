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
            return RectifyColumnSelection(_client.FindEntries(_command), _command.SelectedColumns)
                .Select(x => x.ToObject<T>(_dynamicResults));
        }

        public IEnumerable<T> FindEntries(bool scalarResult)
        {
            return RectifyColumnSelection(_client.FindEntries(_command, scalarResult), _command.SelectedColumns)
                .Select(x => x.ToObject<T>(_dynamicResults));
        }

        public IEnumerable<T> FindEntries(out int totalCount)
        {
            return RectifyColumnSelection(_client.FindEntries(_command.WithInlineCount(), out totalCount), _command.SelectedColumns)
                .Select(x => x.ToObject<T>(_dynamicResults));
        }

        public IEnumerable<T> FindEntries(bool scalarResult, out int totalCount)
        {
            return RectifyColumnSelection(_client.FindEntries(_command.WithInlineCount(), scalarResult, out totalCount), _command.SelectedColumns)
                .Select(x => x.ToObject<T>(_dynamicResults));
        }

        public T FindEntry()
        {
            return RectifyColumnSelection(_client.FindEntry(_command), _command.SelectedColumns)
                .ToObject<T>(_dynamicResults);
        }

        public object FindScalar()
        {
            return _client.FindScalar(_command);
        }

        public T InsertEntry(bool resultRequired = true)
        {
            return _client.InsertEntry(_command, _command.EntryData, resultRequired)
                .ToObject<T>(_dynamicResults);
        }

        public int UpdateEntry()
        {
            if (_command.HasFilter)
                return UpdateEntries();
            else
                return _client.UpdateEntry(_command, _command.KeyValues, _command.EntryData);
        }

        public int UpdateEntries()
        {
            return _client.UpdateEntries(_command, _command.EntryData);
        }

        public int DeleteEntry()
        {
            if (_command.HasFilter)
                return DeleteEntries();
            else
                return _client.DeleteEntry(_command, _command.KeyValues);
        }

        public int DeleteEntries()
        {
            return _client.DeleteEntries(_command);
        }

        public void LinkEntry<U>(U linkedEntryKey, string linkName = null)
        {
            _client.LinkEntry(_command, _command.KeyValues, linkName ?? typeof(U).Name, linkedEntryKey.ToDictionary());
        }

        public void LinkEntry<U>(Expression<Func<T, U>> expression, U linkedEntryKey)
        {
            LinkEntry(linkedEntryKey, ExtractColumnName(expression));
        }

        public void LinkEntry(ODataExpression expression, IDictionary<string, object> linkedEntryKey)
        {
            LinkEntry(linkedEntryKey, expression.AsString());
        }

        public void LinkEntry(ODataExpression expression, ODataEntry linkedEntryKey)
        {
            LinkEntry(linkedEntryKey, expression.AsString());
        }

        public void UnlinkEntry<U>(string linkName = null)
        {
            _client.UnlinkEntry(_command, _command.KeyValues, linkName ?? typeof(U).Name);
        }

        public void UnlinkEntry<U>(Expression<Func<T, U>> expression)
        {
            UnlinkEntry(ExtractColumnName(expression));
        }

        public void UnlinkEntry(ODataExpression expression)
        {
            _client.UnlinkEntry(_command, _command.KeyValues, expression.AsString());
        }

        public IEnumerable<T> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            return RectifyColumnSelection(_client.ExecuteFunction(_command, parameters), _command.SelectedColumns)
                .Select(x => x.ToObject<T>(_dynamicResults));
        }

        public T ExecuteFunctionAsScalar(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsScalar<T>(_command, parameters);
        }

        public T[] ExecuteFunctionAsArray(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsArray<T>(_command, parameters);
        }

        public string GetCommandText()
        {
            return GetCommandTextAsync().Result;
        }
    }
}
