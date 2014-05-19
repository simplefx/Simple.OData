using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Simple.OData.Client
{
    public class HttpRequest
    {
        public string Uri { get; set; }
        public string Method { get; set; }
        public IDictionary<string, object> EntryData { get; set; }
        public HttpContent Content { get; set; }
        public string ContentType { get; set; }
        public string[] Accept { get; set; }
        public ICredentials Credentials { get; set; }
        public bool ReturnContent { get; set; }
    }
}