using System.Collections.Generic;
using System.Linq;
using Simple.OData.Client.Extensions;

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

        public IEnumerable<IDictionary<string, object>> AsEntries()
        {
            return this.Entries 
                ?? (this.Entry != null 
                ? new[] { this.Entry } 
                : new IDictionary<string, object>[] {});
        }

        public IEnumerable<T> AsEntries<T>() where T : class
        {
            return this.AsEntries().Select(x => x.ToObject<T>());
        }

        public IDictionary<string, object> AsEntry()
        {
            var result = this.Entries
                ?? (this.Entry != null
                ? new[] { this.Entry }
                : new IDictionary<string, object>[] { });

            return result != null
                ? result.FirstOrDefault()
                : null;
        }

        public T AsEntry<T>() where T : class
        {
            return this.AsEntry().ToObject<T>();
        }

        public T AsScalar<T>()
        {
            return (T) this.AsEntries().First().First().Value;
        }

        public T[] AsArray<T>()
        {
            return this.AsEntries()
                .SelectMany(x => x.Values)
                .Select(y => (T)y)
                .ToArray();
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