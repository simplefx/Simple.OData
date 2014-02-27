using System.Net;

namespace Simple.OData.Client
{
    public class HttpRequest
    {
        public string Uri { get; set; }
        public string Method { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; }
        public string Accept { get; set; }
        public ICredentials Credentials { get; set; }
        public bool PreAuthenticate { get; set; }
        public bool KeepAlive { get; set; }
    }
}