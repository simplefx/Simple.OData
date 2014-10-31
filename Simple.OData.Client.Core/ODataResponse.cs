using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    public class ODataResponse
    {
        public int StatusCode { get; private set; }
        public IEnumerable<IDictionary<string, object>> Entries { get; private set; }
        public IDictionary<string, object> Entry { get; private set; }
        public long? TotalCount { get; private set; }
        public IList<ODataResponse> Batch { get; private set; }

        private ODataResponse()
        {
        }

        public static ODataResponse FromFeed(IEnumerable<IDictionary<string, object>> entries, long? totalCount = null)
        {
            return new ODataResponse
            {
                Entries = entries,
                TotalCount = totalCount,
            };
        }

        public static ODataResponse FromEntry(IDictionary<string, object> entry)
        {
            return new ODataResponse
            {
                Entry = entry,
            };
        }

        public static ODataResponse FromCollection(IList<object> collection)
        {
            return new ODataResponse
            {
                Entries = collection.Select(x => new Dictionary<string, object>() { { FluentCommand.ResultLiteral, x } }),
            };
        }

        public static ODataResponse FromBatch(IList<ODataResponse> batch)
        {
            return new ODataResponse
            {
                Batch = batch,
            };
        }

        public static ODataResponse FromStatusCode(int statusCode)
        {
            return new ODataResponse
            {
                StatusCode = statusCode,
            };
        }
    }
}