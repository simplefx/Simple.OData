using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IBatchWriter
    {
        Task StartBatchAsync();
        Task<HttpRequestMessage> EndBatchAsync();
        Task<object> CreateOperationRequestMessageAsync(string method, IDictionary<string, object> entryData, Uri uri);
        int LastOperationId { get; }
        string NextContentId();
        string GetContentId(IDictionary<string, object> entryData);
        void MapContentId(IDictionary<string, object> entryData, string contentId);
    }
}