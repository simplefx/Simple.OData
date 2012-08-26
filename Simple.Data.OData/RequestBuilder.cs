using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Simple.Data.OData
{
    public abstract class RequestBuilder
    {
        public string UrlBase { get; private set; }
        public string Host
        {
            get 
            {
                if (string.IsNullOrEmpty(UrlBase)) return null;
                var substr = UrlBase.Substring(UrlBase.IndexOf("//") + 2);
                return substr.Substring(0, substr.IndexOf("/"));
            }
        }
        public HttpWebRequest Request { get; protected set; }

        public RequestBuilder(string urlBase)
        {
            this.UrlBase = urlBase;
        }

        protected string CreateRequestUrl(string command)
        {
            return (UrlBase ?? "http://") + command;
        }

        public abstract void AddTableCommand(string command, string method, string content = null);

        protected abstract void AddContent(WebRequest request, string content);
    }
}
