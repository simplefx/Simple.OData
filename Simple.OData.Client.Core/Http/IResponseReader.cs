using System.Net.Http;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    interface IResponseReader
    {
        Task<ODataResponse> GetResponseAsync(HttpResponseMessage response);
    }
}