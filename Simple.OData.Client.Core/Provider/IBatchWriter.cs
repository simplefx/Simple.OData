using System;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    interface IBatchWriter
    {
        bool IsActive { get; set; }
        
        Task StartBatchAsync();
        Task EndBatchAsync();
        Task<object> CreateOperationRequestMessageAsync(string method, Uri uri);
    }
}