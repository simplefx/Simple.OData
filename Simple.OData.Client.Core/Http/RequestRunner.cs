using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    abstract class RequestRunner : RequestRunnerBase
    {
        public abstract Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(HttpRequest request, bool scalarResult, CancellationToken cancellationToken);
        public abstract Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(HttpRequest request, bool scalarResult, CancellationToken cancellationToken);
        public abstract Task<IDictionary<string, object>> GetEntryAsync(HttpRequest request, CancellationToken cancellationToken);
        public abstract Task<IDictionary<string, object>> InsertEntryAsync(HttpRequest request, CancellationToken cancellationToken);
        public abstract Task<IDictionary<string, object>> UpdateEntryAsync(HttpRequest request, CancellationToken cancellationToken);
        public abstract Task DeleteEntryAsync(HttpRequest request, CancellationToken cancellationToken);
        public abstract Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(HttpRequest request, CancellationToken cancellationToken);
    }
}
