using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public interface IResponseReader
    {
        Task<ODataResponse> GetResponseAsync(HttpResponseMessage responseMessage);
        Task AssignBatchActionResultsAsync(IODataClient client, ODataResponse batchResponse, IList<Func<IODataClient, Task>> actions, IList<int> responseIndexes);
    }
}