using System;
using System.Collections.Generic;

namespace Simple.OData.Client
{
    class BatchRequestRunner : RequestRunner
    {
        private RequestBuilder _requestBuilder;

        public BatchRequestRunner(RequestBuilder requestBuilder)
        {
            _requestBuilder = requestBuilder;
        }

        public override IEnumerable<IDictionary<string, object>> FindEntries(HttpCommand command, bool scalarResult, bool setTotalCount, out int totalCount)
        {
            totalCount = 0;
            return null;
        }

        public override IDictionary<string, object> GetEntry(HttpCommand command)
        {
            return null;
        }

        public override IDictionary<string, object> InsertEntry(HttpCommand command, bool resultRequired)
        {
            return command.OriginalContent;
        }

        public override int UpdateEntry(HttpCommand command)
        {
            return 0;
        }

        public override int DeleteEntry(HttpCommand command)
        {
            return 0;
        }

        public override IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> ExecuteFunction(HttpCommand command)
        {
            return null;
        }
    }
}
