using System;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    interface IBatchWriter
    {
        Task<object> CreateOperationRequestMessageAsync(string method, Uri uri);
    }
}