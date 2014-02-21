using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.OData.Client.TestPcl
{
    public class ClientAdapter
    {
        private readonly ODataClient _client;

        public ClientAdapter(string urlBase)
        {
            _client = new ODataClient(urlBase);
        }

        public Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText)
        {
            return _client.FindEntriesAsync(commandText);
        }

        public Task<IDictionary<string, object>> FindEntryAsync(string commandText)
        {
            return _client.FindEntryAsync(commandText);
        }

        public Task<IDictionary<string, object>> GetEntryAsync(string collection, IDictionary<string, object> entryKey)
        {
            return _client.GetEntryAsync(collection, entryKey);
        }

        public Task<IDictionary<string, object>> InsertEntryAsync(string collection, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            return _client.InsertEntryAsync(collection, entryData, resultRequired);
        }

        public Task<int> UpdateEntryAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            return _client.UpdateEntryAsync(collection, entryKey, entryData);
        }

        public Task<int> DeleteEntryAsync(string collection, IDictionary<string, object> entrykey)
        {
            return _client.DeleteEntryAsync(collection, entrykey);
        }
    }
}
