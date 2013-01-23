using System.Net;


namespace Simple.OData.Client
{
    abstract class RequestBuilder
    {
        public string UrlBase { get; private set; }
#if (NET20 || NET35 || NET40 || SILVERLIGHT)
        public Credentials Credentials { get; private set; }
#endif
        public string Host
        {
            get 
            {
                if (string.IsNullOrEmpty(UrlBase)) return null;
                var substr = UrlBase.Substring(UrlBase.IndexOf("//") + 2);
                return substr.Substring(0, substr.IndexOf("/"));
            }
        }

        public RequestBuilder(string urlBase
#if (NET20 || NET35 || NET40 || SILVERLIGHT)
            , Credentials credentials
#endif
            )
        {
            this.UrlBase = urlBase;
#if (NET20 || NET35 || NET40 || SILVERLIGHT)
            this.Credentials = credentials;
#endif
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
#if (NET20 || NET35 || NET40 || SILVERLIGHT)
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
#endif
            return request;
        }

        public abstract void AddCommandToRequest(HttpCommand command);
        public abstract int GetContentId(object content);
    }
}
