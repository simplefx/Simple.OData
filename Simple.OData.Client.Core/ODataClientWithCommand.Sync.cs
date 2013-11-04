using System;
using System.Collections.Generic;
using System.Linq;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    partial class ODataClientWithCommand : IClientWithCommand
    {
        public IEnumerable<IDictionary<string, object>> FindEntries()
        {
            return RectifySelection(_client.FindEntries(_command.ToString()));
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(bool scalarResult)
        {
            return RectifySelection(_client.FindEntries(_command.ToString(), scalarResult));
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(out int totalCount)
        {
            return RectifySelection(_client.FindEntries(_command.WithInlineCount().ToString(), out totalCount));
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(bool scalarResult, out int totalCount)
        {
            return RectifySelection(_client.FindEntries(_command.WithInlineCount().ToString(), scalarResult, out totalCount));
        }

        public IDictionary<string, object> FindEntry()
        {
            return RectifySelection(_client.FindEntry(_command.ToString()));
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
            return RectifySelection(_client.ExecuteFunction(_command.ToString(), parameters));
        }

        public T ExecuteFunctionAsScalar<T>(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsScalar<T>(_command.ToString(), parameters);
        }

        public T[] ExecuteFunctionAsArray<T>(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsArray<T>(_command.ToString(), parameters);
        }

        private IEnumerable<IDictionary<string, object>> RectifySelection(IEnumerable<IDictionary<string, object>> entries)
        {
            return entries.Select(RectifySelection);
        }

        private IDictionary<string, object> RectifySelection(IDictionary<string, object> entry)
        {
            if (_command.SelectedColumns == null || !_command.SelectedColumns.Any())
            {
                return entry;
            }
            else
            {
                return entry.Where(x => _command.SelectedColumns.Any(y => x.Key.Homogenize() == y.Homogenize())).ToIDictionary();
            }
        }
    }
}
