using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Data.OData;

namespace Simple.OData.Client
{
    class BatchWriterV3 : IBatchWriter
    {
        private readonly ISession _session;
        private ODataBatchWriter _batchWriter;
        private ODataV3RequestMessage _requestMessage;
        private ODataMessageWriter _messageWriter;
        private int _lastContentId;

        public BatchWriterV3(ISession session)
        {
            _session = session;
            _lastContentId = 0;
        }

        public async Task StartBatchAsync()
        {
            _requestMessage = new ODataV3RequestMessage() {Url = new Uri(_session.UrlBase)};
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
            httpRequest.Content.Headers.Add(HttpLiteral.HeaderContentType, _requestMessage.GetHeader(HttpLiteral.HeaderContentType));
            return httpRequest;
        }

        public async Task<object> CreateOperationRequestMessageAsync(string method, Uri uri)
        {
#if SILVERLIGHT
            return _batchWriter.CreateOperationRequestMessage(method, uri);
#else
            return await _batchWriter.CreateOperationRequestMessageAsync(method, uri);
#endif
        }

        public string NextContentId()
        {
            return (++_lastContentId).ToString();
        }
    }
}