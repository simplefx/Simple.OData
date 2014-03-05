using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    abstract class RequestRunner : RequestRunnerBase
    {
        public abstract Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(HttpRequest request, bool scalarResult);
        public abstract Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(HttpRequest request, bool scalarResult);
        public abstract Task<IDictionary<string, object>> GetEntryAsync(HttpRequest request);
        public abstract Task<IDictionary<string, object>> InsertEntryAsync(HttpRequest request);
        public abstract Task<IDictionary<string, object>> UpdateEntryAsync(HttpRequest request);
        public abstract Task DeleteEntryAsync(HttpRequest request);
        public abstract Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(HttpRequest request);
    }
}
