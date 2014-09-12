using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.OData.Core;

namespace Simple.OData.Client
{
#if SILVERLIGH
    class ODataV4ResponseMessage : IODataResponseMessage
#else
    class ODataV4ResponseMessage : IODataResponseMessageAsync
#endif
    {
        private readonly HttpResponseMessage _response;

        public ODataV4ResponseMessage(HttpResponseMessage response)
        {
            _response = response;
        }

        public Task<Stream> GetStreamAsync()
        {
            var responseContent = _response.Content as StreamContent;
            if (responseContent != null)
            {
                return responseContent.ReadAsStreamAsync();
            }
            else
            {
                var completionSource = new TaskCompletionSource<Stream>();
                completionSource.SetResult(Stream.Null);
                return completionSource.Task;
            }
        }

        public string GetHeader(string headerName)
        {
            if (headerName == HttpLiteral.HeaderContentType && _response.Content.Headers.Contains(headerName))
                return _response.Content.Headers.GetValues(headerName).FirstOrDefault();
            else if (_response.Headers.Contains(headerName))
                return _response.Headers.GetValues(headerName).FirstOrDefault();
            else
                return null;
        }

        public Stream GetStream()
        {
            var getStreamTask = GetStreamAsync();
            getStreamTask.Wait();

            return getStreamTask.Result;
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get { return _response.Headers.Select(h => new KeyValuePair<string, string>(h.Key, h.Value.FirstOrDefault())); }
        }

        public void SetHeader(string headerName, string headerValue)
        {
            throw new NotImplementedException();
        }

        public int StatusCode
        {
            get { return (int)_response.StatusCode; }
            set { throw new NotImplementedException(); }
        }
    }
}