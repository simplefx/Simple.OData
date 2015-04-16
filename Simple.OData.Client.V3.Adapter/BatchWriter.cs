using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Data.OData;

#pragma warning disable 1591

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
            _requestMessage = new ODataRequestMessage() { Url = _session.Settings.BaseUri };
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
#if SILVERLIGHT
            if (_pendingChangeSet)
                _batchWriter.WriteEndChangeset();
            _batchWriter.WriteEndBatch();
            var stream = _requestMessage.GetStream();
#else
            if (_pendingChangeSet)
                await _batchWriter.WriteEndChangesetAsync();
            await _batchWriter.WriteEndBatchAsync();
            var stream = await _requestMessage.GetStreamAsync();
#endif
            return CreateMessageFromStream(stream, _requestMessage.Url, _requestMessage.GetHeader);
        }

        protected override Task StartChangesetAsync()
        {
#if SILVERLIGHT
            _batchWriter.WriteStartChangeset();
            return Utils.GetTaskFromResult(0);
#else
            return _batchWriter.WriteStartChangesetAsync();
#endif
        }

        protected override Task EndChangesetAsync()
        {
#if SILVERLIGHT
            _batchWriter.WriteEndChangeset();
            return Utils.GetTaskFromResult(0);
#else
            return _batchWriter.WriteEndChangesetAsync();
#endif
        }

        protected override async Task<object> CreateOperationRequestMessageAsync(string method, string collection, Uri uri, string contentId)
        {
            return await CreateBatchOperationRequestMessageAsync(method, collection, uri, contentId);
        }

        private async Task<ODataBatchOperationRequestMessage> CreateBatchOperationRequestMessageAsync(
            string method, string collection, Uri uri, string contentId)
        {
#if SILVERLIGHT
            var message = _batchWriter.CreateOperationRequestMessage(method, uri);
#else
            var message = await _batchWriter.CreateOperationRequestMessageAsync(method, uri);
#endif

            if (method != RestVerbs.Get && method != RestVerbs.Delete)
                message.SetHeader(HttpLiteral.ContentId, contentId);

            if (_session.Metadata.EntityCollectionRequiresOptimisticConcurrencyCheck(collection) &&
                (method == RestVerbs.Put || method == RestVerbs.Patch || method == RestVerbs.Delete))
            {
                message.SetHeader(HttpLiteral.IfMatch, EntityTagHeaderValue.Any.Tag);
            }

            return message;
        }
    }
}