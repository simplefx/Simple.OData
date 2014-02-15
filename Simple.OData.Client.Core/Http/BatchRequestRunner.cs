using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class BatchRequestRunner : RequestRunner
    {
        public override Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(HttpRequest request, bool scalarResult)
        {
            return null;
        }

        public override Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(HttpRequest request, bool scalarResult)
        {
            return null;
        }

        public override Task<IDictionary<string, object>> GetEntryAsync(HttpRequest request)
        {
            return null;
        }

        public override async Task<IDictionary<string, object>> InsertEntryAsync(HttpRequest request, bool resultRequired = true)
        {
            return await TaskEx.FromResult(request.EntryData);
        }

        public override Task<int> UpdateEntryAsync(HttpRequest request)
        {
            return TaskEx.FromResult(0);
        }

        public override Task<int> DeleteEntryAsync(HttpRequest request)
        {
            return TaskEx.FromResult(0);
        }

        public override Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(HttpRequest request)
        {
            return null;
        }
    }
}
