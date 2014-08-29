using System.Collections.Generic;

namespace Simple.OData.Client
{
    class ODataResponse
    {
        public IEnumerable<IDictionary<string, object>> Entries { get; private set; }
        public IDictionary<string, object> Entry { get; private set; }
        public long? TotalCount { get; private set; }
        public bool IsFeed { get { return this.Entries != null; } }

        public ODataResponse(IEnumerable<IDictionary<string, object>> entries, long? totalCount = null)
        {
            this.Entries = entries;
            this.TotalCount = totalCount;
        }

        public ODataResponse(IDictionary<string, object> entry)
        {
            this.Entry = entry;
        }
    }
}