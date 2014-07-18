using System.Net;

namespace Simple.OData.Client
{
    abstract class RequestBuilder
    {
        public string UrlBase { get; private set; }
        public ICredentials Credentials { get; private set; }
        public string Host
        {
            get
            {
                if (string.IsNullOrEmpty(UrlBase)) return null;
                var substr = UrlBase.Substring(UrlBase.IndexOf("//") + 2);
                return substr.Substring(0, substr.IndexOf("/"));
            }
        }

        public RequestBuilder(string urlBase, ICredentials credentials)
        {
            this.UrlBase = urlBase;
            this.Credentials = credentials;
        }

        protected internal string CreateRequestUrl(string command)
        {
            string url = string.IsNullOrEmpty(UrlBase) ? "http://" : UrlBase;
            if (!url.EndsWith("/"))
                url += "/";
            return url + command;
        }

        protected HttpRequest CreateRequest(string uri)
        {
            var request = new HttpRequest();
            request.Uri = uri;
            request.Credentials = this.Credentials;
            return request;
        }

        public abstract HttpRequest CreateRequest(HttpCommand command, bool returnContent = false, bool checkOptimisticConcurrency = false);
        public abstract int GetContentId(object content);
    }
}
