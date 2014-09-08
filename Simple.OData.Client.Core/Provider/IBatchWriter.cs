using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    interface IBatchWriter
    {
        Task StartBatchAsync();
        Task<HttpRequestMessage> EndBatchAsync();
        Task<object> CreateOperationRequestMessageAsync(string method, Uri uri);
    }
}