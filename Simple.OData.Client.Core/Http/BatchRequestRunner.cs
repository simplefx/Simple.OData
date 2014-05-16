using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class BatchRequestRunner : RequestRunner
    {
        public override Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(HttpRequest request, bool scalarResult, CancellationToken cancellationToken)
        {
            return Utils.GetTaskFromResult(default(IEnumerable<IDictionary<string, object>>));
        }

        public override Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(HttpRequest request, bool scalarResult, CancellationToken cancellationToken)
        {
            return Utils.GetTaskFromResult(default(Tuple<IEnumerable<IDictionary<string, object>>, int>));
        }

        public override Task<IDictionary<string, object>> GetEntryAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            return Utils.GetTaskFromResult(default(IDictionary<string, object>));
        }

        public override async Task<IDictionary<string, object>> InsertEntryAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            return await Utils.GetTaskFromResult(request.EntryData);
        }

        public override async Task<IDictionary<string, object>> UpdateEntryAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            return await Utils.GetTaskFromResult(request.EntryData);
        }

        public override Task DeleteEntryAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            return Utils.GetTaskFromResult(0);
        }

        public override Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(HttpRequest request, CancellationToken cancellationToken)
        {
            return Utils.GetTaskFromResult(default(IEnumerable<IDictionary<string, object>>));
        }
    }
}
