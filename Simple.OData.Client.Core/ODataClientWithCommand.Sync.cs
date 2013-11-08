using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    partial class ODataClientWithCommand
    {
        public IEnumerable<IDictionary<string, object>> FindEntries()
        {
            return RectifyColumnSelection(_client.FindEntries(_command.ToString()), _command.SelectedColumns);
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(bool scalarResult)
        {
            return RectifyColumnSelection(_client.FindEntries(_command.ToString(), scalarResult), _command.SelectedColumns);
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(out int totalCount)
        {
            return RectifyColumnSelection(_client.FindEntries(_command.WithInlineCount().ToString(), out totalCount), _command.SelectedColumns);
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(bool scalarResult, out int totalCount)
        {
            return RectifyColumnSelection(_client.FindEntries(_command.WithInlineCount().ToString(), scalarResult, out totalCount), _command.SelectedColumns);
        }

        public IDictionary<string, object> FindEntry()
        {
            return RectifyColumnSelection(_client.FindEntry(_command.ToString()), _command.SelectedColumns);
        }

        public object FindScalar()
        {
            return _client.FindScalar(_command.ToString());
        }

        public IDictionary<string, object> InsertEntry(bool resultRequired = true)
        {
            return _client.InsertEntry(_command.CollectionName, _command.EntryData, resultRequired);
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

        public void LinkEntry(string linkName, IDictionary<string, object> linkedEntryKey)
        {
            _client.LinkEntry(_command.CollectionName, _command.KeyValues, linkName, linkedEntryKey);
        }

        public void UnlinkEntry(string linkName)
        {
            _client.UnlinkEntry(_command.CollectionName, _command.KeyValues, linkName);
        }

        public IEnumerable<IDictionary<string, object>> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            return RectifyColumnSelection(_client.ExecuteFunction(_command.ToString(), parameters), _command.SelectedColumns);
        }

        public T ExecuteFunctionAsScalar<T>(string functionName, IDictionary<string, object> parameters)
        where T : class, new()
        {
            return _client.ExecuteFunctionAsScalar<T>(_command.ToString(), parameters);
        }

        public T[] ExecuteFunctionAsArray<T>(string functionName, IDictionary<string, object> parameters)
        where T : class, new()
        {
            return _client.ExecuteFunctionAsArray<T>(_command.ToString(), parameters);
        }

        internal static IEnumerable<IDictionary<string, object>> RectifyColumnSelection(IEnumerable<IDictionary<string, object>> entries, IList<string> selectedColumns)
        {
            return entries.Select<IDictionary<string, object>, IDictionary<string, object>>(x => RectifyColumnSelection(x, selectedColumns));
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

    partial class ODataClientWithCommand<T>
    {
        public new IEnumerable<T> FindEntries()
        {
            return RectifyColumnSelection(_client.FindEntries(_command.ToString()), _command.SelectedColumns)
                .Select(x => x.AsObjectOfType<T>());
        }

        public new IEnumerable<T> FindEntries(bool scalarResult)
        {
            return RectifyColumnSelection(_client.FindEntries(_command.ToString(), scalarResult), _command.SelectedColumns)
                .Select(x => x.AsObjectOfType<T>());
        }

        public new IEnumerable<T> FindEntries(out int totalCount)
        {
            return RectifyColumnSelection(_client.FindEntries(_command.WithInlineCount().ToString(), out totalCount), _command.SelectedColumns)
                .Select(x => x.AsObjectOfType<T>());
        }

        public new IEnumerable<T> FindEntries(bool scalarResult, out int totalCount)
        {
            return RectifyColumnSelection(_client.FindEntries(_command.WithInlineCount().ToString(), scalarResult, out totalCount), _command.SelectedColumns)
                .Select(x => x.AsObjectOfType<T>());
        }

        public new T FindEntry()
        {
            return RectifyColumnSelection(_client.FindEntry(_command.ToString()), _command.SelectedColumns)
                .AsObjectOfType<T>();
        }

        public new T InsertEntry(bool resultRequired = true)
        {
            return _client.InsertEntry(_command.CollectionName, _command.EntryData, resultRequired)
                .AsObjectOfType<T>();
        }

        public new void LinkEntry<U>(U linkedEntryKey, string linkName = null)
        {
            _client.LinkEntry(_command.CollectionName, _command.KeyValues, linkName ?? typeof(U).Name, linkedEntryKey.AsDictionary());
        }

        public new void LinkEntry<U>(U linkedEntryKey, Expression<Func<T, U>> expression)
        {
            LinkEntry(linkedEntryKey, ODataCommand.ExtractColumnName(expression));
        }

        public new void UnlinkEntry<U>(string linkName = null)
        {
            _client.UnlinkEntry(_command.CollectionName, _command.KeyValues, linkName ?? typeof(U).Name);
        }

        public new void UnlinkEntry<U>(Expression<Func<T, U>> expression)
        {
            UnlinkEntry(ODataCommand.ExtractColumnName(expression));
        }

        public new IEnumerable<T> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            return RectifyColumnSelection(_client.ExecuteFunction(_command.ToString(), parameters), _command.SelectedColumns)
                .Select(x => x.AsObjectOfType<T>());
        }
    }
}
