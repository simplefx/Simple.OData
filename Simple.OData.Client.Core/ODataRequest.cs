using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Simple.OData.Client
{
    public class ODataRequest
    {
        private HttpRequestMessage _requestMessage;

        public string Uri { get; private set; }
        public ICredentials Credentials { get; private set; }
        public ODataPayloadFormat PayloadFormat { get; private set; }
        public string Method { get; private set; }
        public string CommandText { get; private set; }
        public IDictionary<string, object> EntryData { get; private set; }
        public Stream ContentStream { get; private set; }
        public bool HasContent { get { return this.ContentStream != null; }}
        public int ContentId { get; private set; }

        public HttpRequestMessage RequestMessage
        {
            get { return GetOrCreateRequestMessage(); }
            private set { _requestMessage = value; }
        }

        public string ContentType
        {
            get
            {
                switch (this.PayloadFormat)
                {
                    default:
                    case ODataPayloadFormat.Atom:
                        return this.IsLink ? "application/xml" : "application/atom+xml";

                    case ODataPayloadFormat.Json:
                        return "application/json";
                }
            }
        }

        public string[] Accept
        {
            get
            {
                switch (this.PayloadFormat)
                {
                    default:
                    case ODataPayloadFormat.Atom:
                        if (this.Method == RestVerbs.Get && !this.ReturnsScalarResult || this.ReturnContent)
                            return new[] { "application/text", "application/xml", "application/atom+xml" };
                        else
                            return null;

                    case ODataPayloadFormat.Json:
                        if (this.Method == RestVerbs.Get && !this.ReturnsScalarResult || this.ReturnContent)
                            return new[] { "application/text", "application/xml", "application/json" };
                        else
                            return null;
                }
            }
        }

        public HttpContent GetContent()
        {
            return this.ContentStream != null
                ? new StringContent(Utils.StreamToString(this.ContentStream), Encoding.UTF8, this.ContentType)
                : null;
        }

        public bool IsLink { get; set; }
        public bool ReturnsScalarResult { get; set; }
        public bool ReturnContent { get; set; }
        public bool CheckOptimisticConcurrency { get; set; }

        public object Message { get; set; }

        internal ODataRequest(string method, ISession session, string commandText)
        {
            this.Method = method;
            this.Uri = CreateRequestUrl(session, commandText);
            this.Credentials = session.Credentials;
            this.PayloadFormat = session.PayloadFormat;
            this.CommandText = commandText;
        }

        internal ODataRequest(string method, ISession session, string commandText, HttpRequestMessage requestMessage)
            : this(method, session, commandText)
        {
            this.RequestMessage = requestMessage;
        }

        internal ODataRequest(string method, ISession session, string commandText, IDictionary<string, object> entryData, Stream contentStream)
            : this(method, session, commandText)
        {
            EntryData = entryData;
            ContentStream = contentStream;
        }

        private string CreateRequestUrl(ISession session, string commandText)
        {
            string url = string.IsNullOrEmpty(session.UrlBase) ? "http://" : session.UrlBase;
            if (!url.EndsWith("/"))
                url += "/";
            return url + commandText;
        }

        private HttpRequestMessage GetOrCreateRequestMessage()
        {
            if (_requestMessage != null)
                return _requestMessage;

            _requestMessage = new HttpRequestMessage(new HttpMethod(this.Method), this.Uri);
            _requestMessage.Content = this.HasContent ? this.GetContent() : null;
            return _requestMessage;
        }
    }
}