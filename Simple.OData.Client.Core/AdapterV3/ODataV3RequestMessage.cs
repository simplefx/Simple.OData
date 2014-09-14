using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.OData;

namespace Simple.OData.Client
{
#if SILVERLIGHT
    class ODataV3RequestMessage : IODataRequestMessage
#else
    class ODataV3RequestMessage : IODataRequestMessageAsync
#endif
    {
        private MemoryStream _stream;
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();

        public ODataV3RequestMessage()
        {
        }

        public string GetHeader(string headerName)
        {
            string value;
            return _headers.TryGetValue(headerName, out value) ? value : null;
        }

        public void SetHeader(string headerName, string headerValue)
        {
            _headers.Add(headerName, headerValue);
        }

        public Stream GetStream()
        {
            return _stream ?? (_stream = new MemoryStream());
        }

        public Task<Stream> GetStreamAsync()
        {
            var completionSource = new TaskCompletionSource<Stream>();
            completionSource.SetResult(this.GetStream());
            return completionSource.Task;
        }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get { return _headers; }
        }

        public Uri Url { get; set; }

        public string Method { get; set; }
    }
}