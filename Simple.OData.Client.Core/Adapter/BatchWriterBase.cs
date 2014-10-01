using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public abstract class BatchWriterBase : IBatchWriter
    {
        protected readonly ISession _session;
        private int _lastContentId;
        private readonly Dictionary<IDictionary<string, object>, string> _contentIdMap;

        protected BatchWriterBase(ISession session)
        {
            _session = session;
            _lastContentId = 0;
            _contentIdMap = new Dictionary<IDictionary<string, object>, string>();
        }

        public abstract Task StartBatchAsync();
        public abstract Task<HttpRequestMessage> EndBatchAsync();
        public abstract Task<object> CreateOperationRequestMessageAsync(string method, Uri uri);

        public string NextContentId()
        {
            return (++_lastContentId).ToString();
        }

        public string GetContentId(IDictionary<string, object> entryData)
        {
            string contentId;
            return _contentIdMap.TryGetValue(entryData, out contentId) ? contentId : null;
        }

        public void MapContentId(IDictionary<string, object> entryData, string contentId)
        {
            _contentIdMap.Add(entryData, contentId);
        }
    }
}