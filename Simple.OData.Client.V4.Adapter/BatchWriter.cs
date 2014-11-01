using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.OData.Core;

namespace Simple.OData.Client.V4.Adapter
{
    public class BatchWriter : BatchWriterBase
    {
        private ODataBatchWriter _batchWriter;
        private ODataRequestMessage _requestMessage;
        private ODataMessageWriter _messageWriter;

        public BatchWriter(ISession session)
            : base(session)
        {
        }

        public override async Task StartBatchAsync()
        {
            _requestMessage = new ODataRequestMessage() { Url = new Uri(_session.Settings.UrlBase) };
            _messageWriter = new ODataMessageWriter(_requestMessage);
            _batchWriter = await _messageWriter.CreateODataBatchWriterAsync();
            await _batchWriter.WriteStartBatchAsync();
            await _batchWriter.WriteStartChangesetAsync();
        }

        public override async Task<HttpRequestMessage> EndBatchAsync()
        {
            await _batchWriter.WriteEndChangesetAsync();
            await _batchWriter.WriteEndBatchAsync();
            var stream = await _requestMessage.GetStreamAsync();
            stream.Position = 0;
            var httpRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(_requestMessage.Url + ODataLiteral.Batch),
                Method = HttpMethod.Post,
                Content = new StreamContent(stream),
            };
            httpRequest.Content.Headers.Add(HttpLiteral.ContentType, _requestMessage.GetHeader(HttpLiteral.ContentType));
            return httpRequest;
        }

        public override async Task<object> CreateOperationRequestMessageAsync(string method, IDictionary<string, object> entryData, Uri uri)
        {
            string contentId = null;
            if (method != RestVerbs.Delete)
            {
                contentId = NextContentId();
                MapContentId(entryData, contentId);
            }

            var message = await _batchWriter.CreateOperationRequestMessageAsync(method, uri, contentId);

            if (method != RestVerbs.Delete)
                message.SetHeader(HttpLiteral.ContentId, contentId);

            return message;
        }
    }
}