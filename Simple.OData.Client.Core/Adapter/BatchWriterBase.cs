using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public abstract class BatchWriterBase : IBatchWriter
    {
        protected readonly ISession _session;
        private int _lastOperationId;
        private readonly Dictionary<IDictionary<string, object>, string> _contentIdMap;
        protected bool _pendingChangeSet;

        protected BatchWriterBase(ISession session, IDictionary<object, IDictionary<string, object>> batchEntries)
        {
            _session = session;
            _lastOperationId = 0;
            _contentIdMap = new Dictionary<IDictionary<string, object>, string>();
            this.BatchEntries = batchEntries;
        }

        public abstract Task StartBatchAsync();
        public abstract Task<HttpRequestMessage> EndBatchAsync();
        
        protected abstract Task StartChangesetAsync();
        protected abstract Task EndChangesetAsync();
        protected abstract Task<object> CreateOperationRequestMessageAsync(string method, string collection, Uri uri, string contentId);

        public int LastOperationId { get { return _lastOperationId; } }

        public string NextContentId()
        {
            return (++_lastOperationId).ToString();
        }

        public string GetContentId(IDictionary<string, object> entryData, object linkData)
        {
            string contentId;
            if (!_contentIdMap.TryGetValue(entryData, out contentId) && linkData != null)
            {
                IDictionary<string, object> mappedEntry;
                if (this.BatchEntries.TryGetValue(linkData, out mappedEntry))
                    _contentIdMap.TryGetValue(mappedEntry, out contentId);
            }
            return contentId;
        }

        public void MapContentId(IDictionary<string, object> entryData, string contentId)
        {
            if (entryData != null)
            {
                _contentIdMap.Add(entryData, contentId);
            }
        }

        public IDictionary<object, IDictionary<string, object>> BatchEntries { get; private set; }

        public async Task<object> CreateOperationRequestMessageAsync(string method, string collection, IDictionary<string, object> entryData, Uri uri)
        {
            if (method != RestVerbs.Get && !_pendingChangeSet)
            {
                await StartChangesetAsync();
                _pendingChangeSet = true;
            }
            else if (method == RestVerbs.Get && _pendingChangeSet)
            {
                await EndChangesetAsync();
                _pendingChangeSet = false;
            }

            var contentId = NextContentId();
            if (method != RestVerbs.Get && method != RestVerbs.Delete)
            {
                MapContentId(entryData, contentId);
            }

            return await CreateOperationRequestMessageAsync(method, collection, uri, contentId);
        }

        public bool HasOperations { get; protected set; }

        protected HttpRequestMessage CreateMessageFromStream(Stream stream, Uri requestUrl, Func<string, string> getHeaderFunc)
        {
            _pendingChangeSet = false;
            stream.Position = 0;

            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = Utils.CreateAbsoluteUri(requestUrl.AbsoluteUri, ODataLiteral.Batch),
                Method = HttpMethod.Post,
                Content = new StreamContent(stream),
            };
            httpRequest.Content.Headers.Add(HttpLiteral.ContentType, getHeaderFunc(HttpLiteral.ContentType));
            return httpRequest;
        }
    }
}