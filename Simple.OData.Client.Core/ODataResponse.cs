using System;
using System.Collections.Generic;
using System.Globalization;
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
            if (this.Entries != null)
            {
                return this.Entries.Any() && this.Entries.First().ContainsKey(FluentCommand.ResultLiteral)
                    ? this.Entries.Select(ExtractDictionary)
                    : this.Entries;
            }
            else
            {
                return (this.Entry != null
                ? new[] { ExtractDictionary(this.Entry) }
                : new IDictionary<string, object>[] { });
            }
        }

        public IEnumerable<T> AsEntries<T>() where T : class
        {
            return this.AsEntries().Select(x => x.ToObject<T>());
        }

        public IDictionary<string, object> AsEntry()
        {
            var result = AsEntries();

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
            return (T)Convert.ChangeType(this.AsEntries().First().First().Value, typeof(T), CultureInfo.InvariantCulture);
        }

        public T[] AsArray<T>()
        {
            return this.AsEntries()
                .SelectMany(x => x.Values)
                .Select(x => (T)Convert.ChangeType(x, typeof(T), CultureInfo.InvariantCulture))
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

        private IDictionary<string, object> ExtractDictionary(IDictionary<string, object> value)
        {
            if (value != null && value.Keys.Count == 1 &&
                value.ContainsKey(FluentCommand.ResultLiteral) && 
                value.Values.First() is IDictionary<string, object>)
            {
                return value.Values.First() as IDictionary<string, object>;
            }
            else
            {
                return value;
            }
        }
    }
}