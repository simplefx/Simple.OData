using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public class AnnotatedFeed
    {
        public IList<AnnotatedEntry> Entries { get; private set; }
        public ODataFeedAnnotations Annotations { get; private set; }

        public AnnotatedFeed(IEnumerable<AnnotatedEntry> entries, ODataFeedAnnotations annotations = null)
        {
            this.Entries = entries.ToList();
            this.Annotations = annotations;
        }

        public void SetAnnotations(ODataFeedAnnotations annotations)
        {
            if (this.Annotations == null)
                this.Annotations = annotations;
            else
                this.Annotations.Merge(annotations);
        }
    }

    public class AnnotatedEntry
    {
        public IDictionary<string, object> Data { get; private set; }
        public ODataEntryAnnotations Annotations { get; private set; }
        public ODataFeedAnnotations LinkAnnotations { get; private set; }

        public AnnotatedEntry(IDictionary<string, object> data, ODataEntryAnnotations annotations = null)
        {
            this.Data = data;
            this.Annotations = annotations;
        }

        public void SetAnnotations(ODataEntryAnnotations annotations)
        {
            if (this.Annotations == null)
                this.Annotations = annotations;
            else
                this.Annotations.Merge(annotations);
        }

        public void SetLinkAnnotations(ODataFeedAnnotations annotations)
        {
            if (this.LinkAnnotations == null)
                this.LinkAnnotations = annotations;
            else
                this.LinkAnnotations.Merge(annotations);
        }

        public IDictionary<string, object> GetData(bool includeAnnotations)
        {
            if (includeAnnotations && this.Annotations != null)
            {
                var dataWithAnnotations = new Dictionary<string, object>(this.Data);
                dataWithAnnotations.Add(FluentCommand.AnnotationsLiteral, this.Annotations);
                return dataWithAnnotations;
            }
            else
            {
                return this.Data;
            }
        }
    }

    public class ODataResponse
    {
        public int StatusCode { get; private set; }
        public AnnotatedFeed Feed { get; private set; }
        public IList<ODataResponse> Batch { get; private set; }
        public Exception Exception { get; private set; }

        private ODataResponse()
        {
        }

        public IEnumerable<IDictionary<string, object>> AsEntries(bool includeAnnotations)
        {
            if (this.Feed != null)
            {
                var data = this.Feed.Entries;
                return data.Select(x =>
                    data.Any() && data.First().Data.ContainsKey(FluentCommand.ResultLiteral)
                    ? ExtractDictionary(x, includeAnnotations)
                    : ExtractData(x, includeAnnotations));
            }
            else
            {
                return new IDictionary<string, object>[] { };
            }
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
            Func<IDictionary<string, object>, object> extractScalar = x => (x == null) || !x.Any() ? null : x.Values.First();
            var result = this.AsEntry(false);
            var value = result == null ? null : extractScalar(result);

            return value == null 
                ? default(T) 
                : (T)Utils.Convert(value, typeof(T));
        }

        public T[] AsArray<T>()
        {
            return this.AsEntries(false)
                .SelectMany(x => x.Values)
                .Select(x => (T)Utils.Convert(x, typeof(T)))
                .ToArray();
        }

        public static ODataResponse FromNode(ResponseNode node)
        {
            return new ODataResponse
            {
                Feed = node.Feed ?? new AnnotatedFeed(node.Entry != null ? new[] { node.Entry } : null)
            };
        }

        public static ODataResponse FromProperty(string propertyName, object propertyValue)
        {
            return FromFeed(new[]
            {
                new Dictionary<string, object>() { {propertyName ?? FluentCommand.ResultLiteral, propertyValue} } 
            });
        }

        public static ODataResponse FromValueStream(Stream stream, bool disposeStream = false)
        {
            return FromFeed(new[]
            {
                new Dictionary<string, object>() { {FluentCommand.ResultLiteral, Utils.StreamToString(stream, disposeStream)} } 
            });
        }

        public static ODataResponse FromCollection(IList<object> collection)
        {
            return new ODataResponse
            {
                Feed = new AnnotatedFeed(collection.Select(
                        x => new AnnotatedEntry(new Dictionary<string, object>()
                        {
                            { FluentCommand.ResultLiteral, x }
                        })))
            };
        }

        public static ODataResponse FromBatch(IList<ODataResponse> batch)
        {
            return new ODataResponse
            {
                Batch = batch,
            };
        }

        public static ODataResponse FromStatusCode(int statusCode, Exception exception = null)
        {
            return new ODataResponse
            {
                StatusCode = statusCode,
                Exception = exception,
            };
        }

        public static ODataResponse FromStatusCode(int statusCode, Stream responseStream)
        {
            if (statusCode >= (int)HttpStatusCode.BadRequest)
            {
                var responseContent = Utils.StreamToString(responseStream, true);
                return new ODataResponse
                {
                    StatusCode = statusCode,
                    Exception = WebRequestException.CreateFromStatusCode((HttpStatusCode)statusCode, responseContent),
                };
            }
            else
            {
                return new ODataResponse
                {
                    StatusCode = statusCode,
                };
            }
        }

        public static ODataResponse EmptyFeed
        {
            get { return FromFeed(new Dictionary<string, object>[] { }); }
        }

        private static ODataResponse FromFeed(IEnumerable<IDictionary<string, object>> entries, ODataFeedAnnotations feedAnnotations = null)
        {
            return new ODataResponse
            {
                Feed = new AnnotatedFeed(entries.Select(x => new AnnotatedEntry(x)), feedAnnotations)
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