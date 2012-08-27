using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Data.OData
{
    public class BatchRequestRunner : RequestRunner
    {
        public BatchRequestRunner(RequestBuilder requestBuilder)
            : base(requestBuilder)
        {
        }

        public override IEnumerable<IDictionary<string, object>> FindEntries(bool scalarResult, bool setTotalCount, out int totalCount)
        {
            totalCount = 0;
            return null;
        }

        public override IDictionary<string, object> InsertEntry(bool resultRequired)
        {
            return null;
        }

        public override int UpdateEntry()
        {
            return 0;
        }

        public override int DeleteEntry()
        {
            return 0;
        }
    }
}
