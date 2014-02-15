using System.Collections.Generic;

namespace Simple.OData.Client
{
    class BatchRequestRunner : RequestRunner
    {
        public override IEnumerable<IDictionary<string, object>> FindEntries(HttpRequest request, bool scalarResult, bool setTotalCount, out int totalCount)
        {
            totalCount = 0;
            return null;
        }

        public override IDictionary<string, object> GetEntry(HttpRequest request)
        {
            return null;
        }

        public override IDictionary<string, object> InsertEntry(HttpRequest request, bool resultRequired = true)
        {
            return request.EntryData;
        }

        public override int UpdateEntry(HttpRequest request)
        {
            return 0;
        }

        public override int DeleteEntry(HttpRequest request)
        {
            return 0;
        }

        public override IEnumerable<IDictionary<string, object>> ExecuteFunction(HttpRequest request)
        {
            return null;
        }
    }
}
