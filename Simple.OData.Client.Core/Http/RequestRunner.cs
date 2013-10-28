using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    abstract class RequestRunner : RequestRunnerBase
    {
        public abstract IEnumerable<IDictionary<string, object>> FindEntries(HttpCommand command, bool scalarResult, bool setTotalCount, out int totalCount);
        public abstract IDictionary<string, object> GetEntry(HttpCommand command);
        public abstract IDictionary<string, object> InsertEntry(HttpCommand command, bool resultRequired);
        public abstract int UpdateEntry(HttpCommand command);
        public abstract int DeleteEntry(HttpCommand command);
        public abstract IEnumerable<IDictionary<string, object>> ExecuteFunction(HttpCommand command);

        protected string ExecuteRequestAndGetResponse(HttpWebRequest request)
        {
            using (var response = ExecuteRequest(request))
            {
                if (response != null)
                {
                    var stream = response.GetResponseStream();
                    if (stream != null)
                    {
                        return Utils.StreamToString(stream);
                    }
                }

                return String.Empty;
            }
        }
    }
}
