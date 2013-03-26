using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public partial class ODataClient
    {
        public Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText)
        {
            return Task.Factory.StartNew(() => this.FindEntries(commandText));
        }

        public Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(string commandText, bool scalarResult)
        {
            return Task.Factory.StartNew(() => this.FindEntries(commandText, scalarResult));
        }

        public Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithTotalCountAsync(string commandText)
        {
            return Task.Factory.StartNew(() =>
                {
                    int totalCount;
                    var result = this.FindEntries(commandText, out totalCount);
					return new Tuple<IEnumerable<IDictionary<string, object>>, int>(result, totalCount);
                });
        }

        public Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithTotalCountAsync(string commandText, bool scalarResult)
        {
            return Task.Factory.StartNew(() =>
            {
                int totalCount;
                var result = this.FindEntries(commandText, scalarResult, out totalCount);
                return new Tuple<IEnumerable<IDictionary<string, object>>, int>(result, totalCount);
            });
        }

        public Task<IDictionary<string, object>> FindEntryAsync(string commandText)
        {
            return Task.Factory.StartNew(() => this.FindEntry(commandText));
        }

        public Task<object> FindScalarAsync(string commandText)
        {
            return Task.Factory.StartNew(() => this.FindScalar(commandText));
        }

        public Task<IDictionary<string, object>> GetEntryAsync(string collection, params object[] entryKey)
        {
            return Task.Factory.StartNew(() => this.GetEntry(collection, entryKey));
        }

        public Task<IDictionary<string, object>> GetEntryAsync(string collection, IDictionary<string, object> entryKey)
        {
            return Task.Factory.StartNew(() => this.GetEntry(collection, entryKey));
        }

        public Task<IDictionary<string, object>> InsertEntryAsync(string collection, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            return Task.Factory.StartNew(() => this.InsertEntry(collection, entryData, resultRequired));
        }

        public Task<int> UpdateEntriesAsync(string collection, string commandText, IDictionary<string, object> entryData)
        {
            return Task.Factory.StartNew(() => this.UpdateEntries(collection, commandText, entryData));
        }

        public Task<int> UpdateEntryAsync(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            return Task.Factory.StartNew(() => this.UpdateEntry(collection, entryKey, entryData));
        }

        public Task<int> DeleteEntriesAsync(string collection, string commandText)
        {
            return Task.Factory.StartNew(() => this.DeleteEntries(collection, commandText));
        }

        public Task<int> DeleteEntryAsync(string collection, IDictionary<string, object> entryKey)
        {
            return Task.Factory.StartNew(() => this.DeleteEntry(collection, entryKey));
        }

        public Task LinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            return Task.Factory.StartNew(() => this.LinkEntry(collection, entryKey, linkName, linkedEntryKey));
        }

        public Task UnlinkEntryAsync(string collection, IDictionary<string, object> entryKey, string linkName)
        {
			return Task.Factory.StartNew(() => this.UnlinkEntry(collection, entryKey, linkName));
        }

        public Task<IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters)
        {
            return Task.Factory.StartNew(() => this.ExecuteFunction(functionName, parameters));
        }
    }
}
