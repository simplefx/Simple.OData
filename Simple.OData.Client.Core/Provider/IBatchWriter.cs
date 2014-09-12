using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    interface IBatchWriter
    {
        Task StartBatchAsync();
        Task<HttpRequestMessage> EndBatchAsync();
        Task<object> CreateOperationRequestMessageAsync(string method, Uri uri);
        string NextContentId();
        string GetContentId(IDictionary<string, object> entryData);
        void MapContentId(IDictionary<string, object> entryData, string contentId);
    }
}