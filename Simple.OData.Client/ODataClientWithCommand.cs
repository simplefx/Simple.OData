using System.Collections.Generic;

namespace Simple.OData.Client
{
    class ODataClientWithCommand : IClientWithCommand
    {
        private ODataClient _client;
        private ODataCommand _command;

        public ODataClientWithCommand(ODataClient client)
        {
            _client = client;
            _command = new ODataCommand(this);
        }

        public IDictionary<string, object> FindEntry()
        {
            return _client.FindEntry(_command.ToString());
        }

        public IEnumerable<IDictionary<string, object>> FindEntries()
        {
            return _client.FindEntries(_command.ToString());
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(bool scalarResult)
        {
            return _client.FindEntries(_command.ToString(), scalarResult);
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(bool setTotalCount, out int totalCount)
        {
            return _client.FindEntries(_command.ToString(), false, setTotalCount, out totalCount);
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(bool scalarResult, bool setTotalCount, out int totalCount)
        {
            return _client.FindEntries(_command.ToString(), scalarResult, setTotalCount, out totalCount);
        }

        public IDictionary<string, object> GetEntry(IDictionary<string, object> entryKey)
        {
            return _client.GetEntry(_command.ToString(), entryKey);
        }

        public IDictionary<string, object> InsertEntry(IDictionary<string, object> entryData, bool resultRequired)
        {
            return _client.InsertEntry(_command.ToString(), entryData, resultRequired);
        }

        public int UpdateEntry(IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            return _client.UpdateEntry(_command.ToString(), entryKey, entryData);
        }

        public int DeleteEntry(IDictionary<string, object> entryKey)
        {
            return _client.DeleteEntry(_command.ToString(), entryKey);
        }

        public void LinkEntry(IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            _client.LinkEntry(_command.ToString(), entryKey, linkName, linkedEntryKey);
        }

        public void UnlinkEntry(IDictionary<string, object> entryKey, string linkName)
        {
            _client.UnlinkEntry(_command.ToString(), entryKey, linkName);
        }

        public IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunction(_command.ToString(), parameters);
        }

        public IClientWithCommand Collection(string collectionName)
        {
            return _command.Collection(collectionName);
        }

        public IClientWithCommand Filter(string filter)
        {
            return _command.Filter(filter);
        }

        public IClientWithCommand Skip(int count)
        {
            return _command.Skip(count);
        }

        public IClientWithCommand Top(int count)
        {
            return _command.Top(count);
        }

        public IClientWithCommand Expand(IEnumerable<string> associations)
        {
            return _command.Expand(associations);
        }

        public IClientWithCommand Expand(params string[] associations)
        {
            return _command.Expand(associations);
        }

        public IClientWithCommand Select(IEnumerable<string> columns)
        {
            return _command.Select(columns);
        }

        public IClientWithCommand Select(params string[] columns)
        {
            return _command.Select(columns);
        }

        public IClientWithCommand OrderBy(IEnumerable<string> columns, bool @descending = false)
        {
            return _command.OrderBy(columns, descending);
        }

        public IClientWithCommand OrderBy(params string[] columns)
        {
            return _command.OrderBy(columns);
        }

        public IClientWithCommand OrderByDescending(IEnumerable<string> columns)
        {
            return _command.OrderByDescending(columns);
        }

        public IClientWithCommand OrderByDescending(params string[] columns)
        {
            return _command.OrderByDescending(columns);
        }
    }
}
