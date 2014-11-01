using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Data.OData;

namespace Simple.OData.Client.V3.Adapter
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
            _requestMessage = new ODataRequestMessage() {Url = new Uri(_session.Settings.UrlBase)};
            _messageWriter = new ODataMessageWriter(_requestMessage);
#if SILVERLIGHT
            _batchWriter = _messageWriter.CreateODataBatchWriter();
            _batchWriter.WriteStartBatch();
#else
            _batchWriter = await _messageWriter.CreateODataBatchWriterAsync();
            await _batchWriter.WriteStartBatchAsync();
#endif
        }

        public override async Task<HttpRequestMessage> EndBatchAsync()
        {
            Stream stream;
#if SILVERLIGHT
            if (_pendingChangeSet)
                _batchWriter.WriteEndChangeset();
            _batchWriter.WriteEndBatch();
            stream = _requestMessage.GetStream();
#else
            if (_pendingChangeSet)
                await _batchWriter.WriteEndChangesetAsync();
            await _batchWriter.WriteEndBatchAsync();
            stream = await _requestMessage.GetStreamAsync();
#endif
            _pendingChangeSet = false;
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
            if (method != RestVerbs.Get && !_pendingChangeSet)
            {
#if SILVERLIGHT
                _batchWriter.WriteStartChangeset();
#else
                await _batchWriter.WriteStartChangesetAsync();
#endif
                _pendingChangeSet = true;
            }
            else if (method == RestVerbs.Get && _pendingChangeSet)
            {
#if SILVERLIGHT
                _batchWriter.WriteEndChangeset();
#else
                await _batchWriter.WriteEndChangesetAsync();
#endif
                _pendingChangeSet = false;
            }

            var contentId = NextContentId();
            if (method != RestVerbs.Get && method != RestVerbs.Delete)
            {
                MapContentId(entryData, contentId);
            }

#if SILVERLIGHT
            var message = _batchWriter.CreateOperationRequestMessage(method, uri);
#else
            var message = await _batchWriter.CreateOperationRequestMessageAsync(method, uri);
#endif
            if (method != RestVerbs.Get && method != RestVerbs.Delete)
                message.SetHeader(HttpLiteral.ContentId, contentId);

            return message;
        }
    }
}