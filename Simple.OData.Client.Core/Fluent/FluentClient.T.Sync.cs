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

        public new T FindEntry()
        {
            return RectifyColumnSelection(_client.FindEntry(_command.ToString()), _command.SelectedColumns)
                .ToObject<T>();
        }

        public new T InsertEntry(bool resultRequired = true)
        {
            return _client.InsertEntry(_command.CollectionName, _command.EntryData, resultRequired)
                .ToObject<T>();
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
    }
}