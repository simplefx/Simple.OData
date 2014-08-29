using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class ResponseReader : IResponseReader
    {
        private readonly Func<HttpResponseMessage, IProviderResponseReader> _responseReaderFunc;

        public ResponseReader(Func<HttpResponseMessage, IProviderResponseReader> responseReaderFunc)
        {
            _responseReaderFunc = responseReaderFunc;
        }

        public Task<ODataResponse> GetResponseAsync(HttpResponseMessage response)
        {
            return _responseReaderFunc(response).GetResponseAsync();
        }
    }
}