using System.Net.Http;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IResponseReader
    {
        Task<ODataResponse> GetResponseAsync(HttpResponseMessage responseMessage, bool includeResourceTypeInEntryProperties = false);
    }
}