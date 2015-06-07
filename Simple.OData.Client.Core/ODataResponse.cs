using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class AnnotatedFeed
    {
        public IList<AnnotatedEntry> Entries { get; set; }
        public ODataFeedAnnotations Annotations { get; set; }

        public AnnotatedFeed() { this.Entries = new List<AnnotatedEntry>(); }
    }

    public class AnnotatedEntry
    {
        public IDictionary<string, object> Data { get; set; }
        public ODataEntryAnnotations Annotations { get; set; }

        public AnnotatedEntry() { this.Data = new Dictionary<string, object>(); }
    }

    public class ODataResponse
    {
        public int StatusCode { get; private set; }
        public AnnotatedFeed Feed { get; private set; }
        public IList<ODataResponse> Batch { get; private set; }

        private ODataResponse()
        {
        }

        public IEnumerable<IDictionary<string, object>> AsEntries(bool includeAnnotations)
        {
            if (this.Feed == null)
                return null;

            var data = this.Feed.Entries;
            return data.Select(x =>
                data.Any() && data.First().Data.ContainsKey(FluentCommand.ResultLiteral)
                ? ExtractDictionary(x, includeAnnotations)
                : ExtractData(x, includeAnnotations));
        }

        public IDictionary<string, object> AsEntry(bool includeAnnotations)
        {
            var result = AsEntries(includeAnnotations);

            return result != null
                ? result.FirstOrDefault()
                : null;
        }

        public T AsScalar<T>()
        {
            var entry = this.AsEntry(false);
            return entry == null 
                ? default(T)
                : (T)Convert.ChangeType(entry.First().Value, typeof(T), CultureInfo.InvariantCulture);
        }

        public T[] AsArray<T>()
        {
            return this.AsEntries(false)
                .SelectMany(x => x.Values)
                .Select(x => (T)Convert.ChangeType(x, typeof(T), CultureInfo.InvariantCulture))
                .ToArray();
        }

        public static ODataResponse FromNode(ResponseNode node)
        {
            return new ODataResponse
            {
                Feed = node.Feed ?? new AnnotatedFeed() { Entries = node.Entry != null ? new[] { node.Entry } : null }
            };
        }

        public static ODataResponse FromProperty(string propertyName, object propertyValue)
        {
            return FromFeed(new[]
            {
                new Dictionary<string, object>() { {propertyName ?? FluentCommand.ResultLiteral, propertyValue} } 
            });
        }

        public static ODataResponse FromValueStream(Stream stream)
        {
            return FromFeed(new[]
            {
                new Dictionary<string, object>() { {FluentCommand.ResultLiteral, Utils.StreamToString(stream)} } 
            });
        }

        public static ODataResponse FromCollection(IList<object> collection)
        {
            return new ODataResponse
            {
                Feed = new AnnotatedFeed
                {
                    Entries = collection.Select(x => new AnnotatedEntry
                    {
                        Data = new Dictionary<string, object>() { { FluentCommand.ResultLiteral, x } }
                    })
                    .ToList()
                }
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

        public static ODataResponse EmptyFeed
        {
            get { return FromFeed(new Dictionary<string, object>[] { }); }
        }

        private static ODataResponse FromFeed(IEnumerable<IDictionary<string, object>> entries, ODataFeedAnnotations feedAnnotations = null)
        {
            return new ODataResponse
            {
                Feed = new AnnotatedFeed
                {
                    Entries = entries.Select(x => new AnnotatedEntry { Data = x }).ToList(),
                    Annotations = feedAnnotations,
                }
            };
        }

        private IDictionary<string, object> ExtractData(AnnotatedEntry entry, bool includeAnnotations)
        {
            if (entry == null || entry.Data == null)
                return null;

            return includeAnnotations ? DataWithAnnotations(entry.Data, entry.Annotations) : entry.Data;
        }

        private IDictionary<string, object> ExtractDictionary(AnnotatedEntry entry, bool includeAnnotations)
        {
            if (entry == null || entry.Data == null)
                return null;

            var data = entry.Data;
            if (data.Keys.Count == 1 && data.ContainsKey(FluentCommand.ResultLiteral) &&
                data.Values.First() is IDictionary<string, object>)
            {
                return data.Values.First() as IDictionary<string, object>;
            }
            else if (includeAnnotations)
            {
                return DataWithAnnotations(data, entry.Annotations);
            }
            else
            {
                return data;
            }
        }

        private IDictionary<string, object> DataWithAnnotations(
            IDictionary<string, object> data, ODataEntryAnnotations annotations)
        {
            var dataWithAnnotations = new Dictionary<string, object>(data);
            dataWithAnnotations.Add(FluentCommand.AnnotationsLiteral, annotations);
            return dataWithAnnotations;
        }
    }
}