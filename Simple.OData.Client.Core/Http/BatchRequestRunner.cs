using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class BatchRequestRunner : RequestRunner
    {
        public override Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(HttpRequest request, bool scalarResult)
        {
            return Utils.GetTaskFromResult(default(IEnumerable<IDictionary<string, object>>));
        }

        public override Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(HttpRequest request, bool scalarResult)
        {
            return Utils.GetTaskFromResult(default(Tuple<IEnumerable<IDictionary<string, object>>, int>));
        }

        public override Task<IDictionary<string, object>> GetEntryAsync(HttpRequest request)
        {
            return Utils.GetTaskFromResult(default(IDictionary<string, object>));
        }

        public override async Task<IDictionary<string, object>> InsertEntryAsync(HttpRequest request)
        {
            return await Utils.GetTaskFromResult(request.EntryData);
        }

        public override async Task<IDictionary<string, object>> UpdateEntryAsync(HttpRequest request)
        {
            return await Utils.GetTaskFromResult(request.EntryData);
        }

        public override Task DeleteEntryAsync(HttpRequest request)
        {
            return Utils.GetTaskFromResult(0);
        }

        public override Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(HttpRequest request)
        {
            return Utils.GetTaskFromResult(default(IEnumerable<IDictionary<string, object>>));
        }
    }
}
