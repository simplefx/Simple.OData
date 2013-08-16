using System.Collections.Generic;

namespace Simple.OData.Client
{
    partial class ODataClientWithCommand : IClientWithCommand
    {
        public IEnumerable<IDictionary<string, object>> FindEntries()
        {
            return _client.FindEntries(_command.ToString());
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(bool scalarResult)
        {
            return _client.FindEntries(_command.ToString(), scalarResult);
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(out int totalCount)
        {
            var result = _client.FindEntries(_command.WithInlineCount().ToString(), out totalCount);
            return result;
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(bool scalarResult, out int totalCount)
        {
            var result = _client.FindEntries(_command.WithInlineCount().ToString(), scalarResult, out totalCount);
            return result;
        }

        public IDictionary<string, object> FindEntry()
        {
            return _client.FindEntry(_command.ToString());
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
            return _client.ExecuteFunction(_command.ToString(), parameters);
        }

        public T ExecuteFunctionAsScalar<T>(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsScalar<T>(_command.ToString(), parameters);
        }

        public T[] ExecuteFunctionAsArray<T>(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsArray<T>(_command.ToString(), parameters);
        }
    }
}
