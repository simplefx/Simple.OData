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
        private ODataMessageWriter _messageWriter;

        public BatchWriterV3(ISession session)
        {
            _session = session;
        }

        public async Task StartBatch()
        {
            _messageWriter = new ODataMessageWriter(
                new ODataV3RequestMessage(ODataPayloadKind.Batch, new Uri(_session.UrlBase), _session.Credentials));
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

        public async Task EndBatch()
        {
#if SILVERLIGHT
            _batchWriter.WriteEndChangeset();
            _batchWriter.WriteEndBatch();
#else
            await _batchWriter.WriteEndChangesetAsync();
            await _batchWriter.WriteEndBatchAsync();
#endif
        }

        public async Task<object> CreateOperationRequestMessageAsync(string method, Uri uri)
        {
#if SILVERLIGHT
            return _batchWriter.CreateOperationRequestMessage(method, uri);
#else
            return await _batchWriter.CreateOperationRequestMessageAsync(method, uri);
#endif
        }
    }
}