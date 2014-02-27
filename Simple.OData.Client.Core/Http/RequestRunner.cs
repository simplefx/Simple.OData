using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    abstract class RequestRunner : RequestRunnerBase
    {
        public abstract Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(HttpCommand command, bool scalarResult);
        public abstract Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(HttpCommand command, bool scalarResult);
        public abstract Task<IDictionary<string, object>> GetEntryAsync(HttpCommand command);
        public abstract Task<IDictionary<string, object>> InsertEntryAsync(HttpCommand command, bool resultRequired);
        public abstract Task<int> UpdateEntryAsync(HttpCommand command);
        public abstract Task<int> DeleteEntryAsync(HttpCommand command);
        public abstract Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(HttpCommand command);

        protected async Task<string> ExecuteRequestAndGetResponseAsync(HttpRequest request)
        {
            using (var response = await ExecuteRequestAsync(request))
            {
                var stream = response.GetResponseStream();
                if (stream != null)
                {
                    return Utils.StreamToString(stream);
                }

                return String.Empty;
            }
        }
    }
}
