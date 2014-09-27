using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.OData.Core;

namespace Simple.OData.Client.V4.Adapter
{
    public class BatchWriter : IBatchWriter
    {
        private readonly ISession _session;
        private ODataBatchWriter _batchWriter;
        private ODataRequestMessage _requestMessage;
        private ODataMessageWriter _messageWriter;
        private int _lastContentId;
        private readonly Dictionary<IDictionary<string, object>, string> _contentIdMap;

        public BatchWriter(ISession session)
        {
            _session = session;
            _lastContentId = 0;
            _contentIdMap = new Dictionary<IDictionary<string, object>, string>();
        }

        public async Task StartBatchAsync()
        {
            _requestMessage = new ODataRequestMessage() { Url = new Uri(_session.UrlBase) };
            _messageWriter = new ODataMessageWriter(_requestMessage);
#if SILVERLIGHT
            _batchWriter = _messageWriter.CreateODataBatchWriter();
            _batchWriter.WriteStartBatch();
            _batchWriter.WriteStartChangeset();
#else
            _batchWriter = await _messageWriter.CreateODataBatchWriterAsync();
            await _batchWriter.WriteStartBatchAsync();
            await _batchWriter.WriteStartChangesetAsync();
#endif
        }

        public async Task<HttpRequestMessage> EndBatchAsync()
        {
#if SILVERLIGHT
            _batchWriter.WriteEndChangeset();
            _batchWriter.WriteEndBatch();
#else
            await _batchWriter.WriteEndChangesetAsync();
            await _batchWriter.WriteEndBatchAsync();
#endif
            _requestMessage.GetStream().Position = 0;
            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(_requestMessage.Url + ODataLiteral.Batch),
                Method = HttpMethod.Post,
                Content = new StreamContent(_requestMessage.GetStream()),
            };
            httpRequest.Content.Headers.Add(HttpLiteral.ContentType, _requestMessage.GetHeader(HttpLiteral.ContentType));
            return httpRequest;
        }

        public async Task<object> CreateOperationRequestMessageAsync(string method, Uri uri)
        {
#if SILVERLIGHT
            return _batchWriter.CreateOperationRequestMessage(method, uri, null);
#else
            return await _batchWriter.CreateOperationRequestMessageAsync(method, uri, null);
#endif
        }

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