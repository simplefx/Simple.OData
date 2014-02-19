using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class BatchRequestRunner : RequestRunner
    {
        private RequestBuilder _requestBuilder;

        public BatchRequestRunner(RequestBuilder requestBuilder)
        {
            _requestBuilder = requestBuilder;
        }

        public override Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(HttpCommand command, bool scalarResult)
        {
            return null;
        }

        public override Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(HttpCommand command, bool scalarResult)
        {
            return null;
        }

        public override Task<IDictionary<string, object>> GetEntryAsync(HttpCommand command)
        {
            return null;
        }

        public override async Task<IDictionary<string, object>> InsertEntryAsync(HttpCommand command, bool resultRequired = true)
        {
            return await TaskEx.FromResult(command.OriginalContent);
        }

        public override Task<int> UpdateEntryAsync(HttpCommand command)
        {
            return TaskEx.FromResult(0);
        }

        public override Task<int> DeleteEntryAsync(HttpCommand command)
        {
            return TaskEx.FromResult(0);
        }

        public override Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(HttpCommand command)
        {
            return null;
        }
    }
}
