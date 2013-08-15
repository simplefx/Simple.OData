using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    partial class ODataClientWithCommand : IClientWithCommand
    {
        public Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync()
        {
            return _client.FindEntriesAsync(_command.ToString());
        }

        public Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(bool scalarResult)
        {
            return _client.FindEntriesAsync(_command.ToString(), scalarResult);
        }

        public Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync()
        {
            return _client.FindEntriesWithCountAsync(_command.WithInlineCount().ToString());
        }

        public Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(bool scalarResult)
        {
            return _client.FindEntriesWithCountAsync(_command.WithInlineCount().ToString(), scalarResult);
        }

        public Task<IDictionary<string, object>> FindEntryAsync()
        {
            return _client.FindEntryAsync(_command.ToString());
        }

        public Task<object> FindScalarAsync()
        {
            return _client.FindScalarAsync(_command.ToString());
        }

        public Task<IDictionary<string, object>> InsertEntryAsync(bool resultRequired = true)
        {
            return _client.InsertEntryAsync(_command.CollectionName, _command.EntryData, resultRequired);
        }

        public Task<int> UpdateEntryAsync()
        {
            if (_command.HasFilter)
                return UpdateEntriesAsync();
            else
                return _client.UpdateEntryAsync(_command.CollectionName, _command.KeyValues, _command.EntryData);
        }

        public Task<int> UpdateEntriesAsync()
        {
            return _client.UpdateEntriesAsync(_command.CollectionName, _command.ToString(), _command.EntryData);
        }

        public Task<int> DeleteEntryAsync()
        {
            if (_command.HasFilter)
                return DeleteEntriesAsync();
            else
                return _client.DeleteEntryAsync(_command.CollectionName, _command.KeyValues);
        }

        public Task<int> DeleteEntriesAsync()
        {
            return _client.DeleteEntriesAsync(_command.CollectionName, _command.ToString());
        }

        public Task LinkEntryAsync(string linkName, IDictionary<string, object> linkedEntryKey)
        {
            return _client.LinkEntryAsync(_command.CollectionName, _command.KeyValues, linkName, linkedEntryKey);
        }

        public Task UnlinkEntryAsync(string linkName)
        {
            return _client.UnlinkEntryAsync(_command.CollectionName, _command.KeyValues, linkName);
        }

        public Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsync(_command.ToString(), parameters);
        }
    }
}
