using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    abstract class RequestRunner : RequestRunnerBase
    {
        public abstract Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(HttpRequest request, bool scalarResult);
        public abstract Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(HttpRequest request, bool scalarResult);
        public abstract Task<IDictionary<string, object>> GetEntryAsync(HttpRequest request);
        public abstract Task<IDictionary<string, object>> InsertEntryAsync(HttpRequest request, bool resultRequired);
        public abstract Task<int> UpdateEntryAsync(HttpRequest request);
        public abstract Task<int> DeleteEntryAsync(HttpRequest request);
        public abstract Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(HttpRequest request);
    }
}
