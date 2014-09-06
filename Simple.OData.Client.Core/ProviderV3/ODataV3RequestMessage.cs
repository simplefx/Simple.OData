using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Data.OData;

namespace Simple.OData.Client
{
#if SILVERLIGHT
    class ODataV3RequestMessage : IODataRequestMessage, IDisposable
#else
    class ODataV3RequestMessage : IODataRequestMessageAsync, IDisposable
#endif
    {
        private bool _disposed = false;
        private MemoryStream _requestStream;
        private readonly ICredentials _credentials;
        private readonly HttpRequestMessage _request;
        private readonly ODataPayloadKind _payloadKind;

        public ODataV3RequestMessage(ODataPayloadKind payloadKind, Uri url, ICredentials credentials)
        {
            _request = new HttpRequestMessage() { RequestUri = url };
            _credentials = credentials;
            _request.Headers.Add("DataServiceVersion", "3.0");
            _payloadKind = payloadKind;
        }

        public Task<Stream> GetStreamAsync()
        {
            if (_requestStream == null)
                _requestStream = new MemoryStream();

            var completionSource = new TaskCompletionSource<Stream>();
            completionSource.SetResult(_requestStream);
            return completionSource.Task;
        }

        public string GetHeader(string headerName)
        {
            if (headerName == "Content-Type")
            {
                switch (_payloadKind)
                {
                    case ODataPayloadKind.EntityReferenceLink:
                        return "application/xml";
                    default:
                        return "application/atom+xml";
                }
            } 
            return _request.Headers.GetValues(headerName).FirstOrDefault();
        }

        public Stream GetStream()
        {
            if (_requestStream == null)
                _requestStream = new MemoryStream();

            return _requestStream;
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get { return _request.Headers.Select(h => new KeyValuePair<string, string>(h.Key, h.Value.FirstOrDefault())); }
        }

        public string Method
        {
            get { return _request.Method.Method; }

            set { _request.Method = new HttpMethod(value); }
        }

        public void SetHeader(string headerName, string headerValue)
        {
            if (_request.Headers.Contains(headerName))
                _request.Headers.Remove(headerName);

            _request.Headers.Add(headerName, headerValue);
        }

        public Uri Url
        {
            get { return _request.RequestUri; }

            set { throw new ArgumentException("Request Uri cannot be changed."); }
        }

        public async Task<ODataV3ResponseMessage> GetResponseAsync()
        {
            using (var clientHandler = new HttpClientHandler() { Credentials = _credentials })
            {
                using (var requestClient = new HttpClient(clientHandler))
                {
                    //if (_requestStream != null)
                    //    _request.Content = new PushStreamContent(stream => _requestStream.WriteTo(stream));

                    var completionOption = (_request.Method == HttpMethod.Get || _request.Method == HttpMethod.Trace) ? HttpCompletionOption.ResponseContentRead : HttpCompletionOption.ResponseHeadersRead;
                    var response = await requestClient.SendAsync(_request, completionOption);

                    return new ODataV3ResponseMessage(response);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_requestStream != null)
                    {
                        _requestStream.Dispose();
                        _requestStream = null;
                    }
                }
                _disposed = true;
            }
        }

        ~ODataV3RequestMessage()
        {
            Dispose(false);
        }
    }
}