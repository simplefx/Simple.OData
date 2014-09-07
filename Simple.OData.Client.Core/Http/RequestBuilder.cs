using System;
using System.Net;

namespace Simple.OData.Client
{
    abstract class RequestBuilder
    {
        public ISession Session { get; private set; }
        public string Host
        {
            get
            {
                if (string.IsNullOrEmpty(this.Session.UrlBase)) return null;
                var substr = this.Session.UrlBase.Substring(this.Session.UrlBase.IndexOf("//") + 2);
                return substr.Substring(0, substr.IndexOf("/"));
            }
        }

        protected RequestBuilder(ISession session)
        {
            this.Session = session;
        }

        protected internal string CreateRequestUrl(string command)
        {
            string url = string.IsNullOrEmpty(this.Session.UrlBase) ? "http://" : this.Session.UrlBase;
            if (!url.EndsWith("/"))
                url += "/";
            return url + command;
        }

        protected HttpRequest CreateRequest(string uri)
        {
            var request = new HttpRequest();
            request.Uri = uri;
            request.Credentials = this.Session.Credentials;
            return request;
        }

        public abstract HttpRequest CreateRequest(HttpCommand command, bool returnContent = false, bool checkOptimisticConcurrency = false);
        public virtual Lazy<IBatchWriter> GetDeferredBatchWriter() { return null; }
        public virtual int GetBatchContentId(object content) { return 0; }
    }
}
