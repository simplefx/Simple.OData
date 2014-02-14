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
        public abstract IEnumerable<IDictionary<string, object>> FindEntries(HttpRequest request, bool scalarResult, bool setTotalCount, out int totalCount);
        public abstract IDictionary<string, object> GetEntry(HttpRequest request);
        public abstract IDictionary<string, object> InsertEntry(HttpRequest request, bool resultRequired);
        public abstract int UpdateEntry(HttpRequest request);
        public abstract int DeleteEntry(HttpRequest request);
        public abstract IEnumerable<IDictionary<string, object>> ExecuteFunction(HttpRequest request);
    }
}
