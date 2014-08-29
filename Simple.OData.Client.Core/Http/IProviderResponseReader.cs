using System.Threading.Tasks;

namespace Simple.OData.Client
{
    interface IProviderResponseReader
    {
        Task<ODataResponse> GetResponseAsync();
    }
}