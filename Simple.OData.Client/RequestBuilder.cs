
using System.Net;

namespace Simple.OData.Client
{
    abstract class RequestBuilder
    {
        public string UrlBase { get; private set; }
        public Credentials Credentials { get; private set; }
        public string Host
        {
            get 
            {
                if (string.IsNullOrEmpty(UrlBase)) return null;
                var substr = UrlBase.Substring(UrlBase.IndexOf("//") + 2);
                return substr.Substring(0, substr.IndexOf("/"));
            }
        }

        public RequestBuilder(string urlBase, Credentials credentials)
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

        protected HttpWebRequest CreateWebRequest(string uri)
        {
            var request = (HttpWebRequest) WebRequest.Create(uri);
            bool authenticate = false;
            if (this.Credentials.IntegratedSecurity)
            {
                request.Credentials = CredentialCache.DefaultNetworkCredentials;
                authenticate = true;
            }
            else if (!string.IsNullOrEmpty(this.Credentials.User))
            {
                request.Credentials = new NetworkCredential(this.Credentials.User, this.Credentials.Password, this.Credentials.Domain);
            }
            if (authenticate)
            {
                request.PreAuthenticate = true;
                request.KeepAlive = true;
            }
            return request;
        }

        public abstract void AddCommandToRequest(HttpCommand command);
        public abstract int GetContentId(object content);
    }
}
