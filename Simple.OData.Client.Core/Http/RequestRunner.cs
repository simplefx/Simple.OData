using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    abstract class RequestRunner : RequestRunnerBase
    {
        public abstract Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(ODataRequest request, bool scalarResult, CancellationToken cancellationToken);
        public abstract Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(ODataRequest request, bool scalarResult, CancellationToken cancellationToken);
        public abstract Task<IDictionary<string, object>> GetEntryAsync(ODataRequest request, CancellationToken cancellationToken);
        public abstract Task<IDictionary<string, object>> InsertEntryAsync(ODataRequest request, CancellationToken cancellationToken);
        public abstract Task<IDictionary<string, object>> UpdateEntryAsync(ODataRequest request, CancellationToken cancellationToken);
        public abstract Task DeleteEntryAsync(ODataRequest request, CancellationToken cancellationToken);
        public abstract Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(ODataRequest request, CancellationToken cancellationToken);
    }
}
