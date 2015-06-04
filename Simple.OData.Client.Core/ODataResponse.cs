using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class ODataResponse
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

        public int StatusCode { get; private set; }
        public AnnotatedFeed Feed { get; private set; }
        public AnnotatedEntry Entry { get; private set; }
        public IList<ODataResponse> Batch { get; private set; }
        public string DynamicPropertiesContainerName { get; private set; }

        private ODataResponse()
        {
        }

        public IEnumerable<IDictionary<string, object>> AsEntries(bool includeAnnotationsInResults = false)
        {
            if (this.Feed != null)
            {
                var data = this.Feed.Entries;
                return data.Any() && data.First().Data.ContainsKey(FluentCommand.ResultLiteral)
                    ? data.Select(x => ExtractDictionary(x, includeAnnotationsInResults))
                    : data.Select(x => x.Data);
            }
            else
            {
                return (this.Entry != null
                ? new[] { ExtractDictionary(this.Entry, includeAnnotationsInResults) }
                : new IDictionary<string, object>[] { });
            }
        }

        public IEnumerable<T> AsEntries<T>(string dynamicPropertiesContainerName) where T : class
        {
            return this.AsEntries().Select(x => x.ToObject<T>(dynamicPropertiesContainerName));
        }

        public IDictionary<string, object> AsEntry()
        {
            var result = AsEntries();

            return result != null
                ? result.FirstOrDefault()
                : null;
        }

        public T AsEntry<T>(string dynamicPropertiesContainerName) where T : class
        {
            return this.AsEntry().ToObject<T>(dynamicPropertiesContainerName);
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

        public static ODataResponse FromNode(ResponseNode node)
        {
            if (node.Feed != null)
            {
                return new ODataResponse { Feed = node.Feed };
            }
            else
            {
                return new ODataResponse { Entry = node.Entry };
            }
        }

        public static ODataResponse FromFeed(IEnumerable<IDictionary<string, object>> entries, ODataFeedAnnotations feedAnnotations = null)
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

        public static ODataResponse FromEntry(IDictionary<string, object> entry, ODataEntryAnnotations entryAnnotations = null)
        {
            return new ODataResponse
            {
                Entry = new AnnotatedEntry
                {
                    Data = entry,
                    Annotations = entryAnnotations,
                }
            };
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

        private IDictionary<string, object> ExtractDictionary(AnnotatedEntry entry, bool includeAnnotationsInResults)
        {
            if (entry == null || entry.Data == null)
                return null;

            var data = entry.Data;
            if (data.Keys.Count == 1 && data.ContainsKey(FluentCommand.ResultLiteral) && 
                data.Values.First() is IDictionary<string, object>)
            {
                return data.Values.First() as IDictionary<string, object>;
            }
            else if (includeAnnotationsInResults)
            {
                data.Add(FluentCommand.AnnotationsLiteral, entry.Annotations);
                return data;
            }
            else
            {
                return data;
            }
        }
    }
}