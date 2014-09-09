using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Simple.OData.Client
{
    class ODataRequest
    {
        private HttpRequestMessage _requestMessage;

        public string Uri { get; private set; }
        public ICredentials Credentials { get; private set; }
        public string Method { get; private set; }
        public string CommandText { get; private set; }
        public IDictionary<string, object> EntryData { get; private set; }
        public string FormattedContent { get; private set; }
        public bool HasContent { get { return this.FormattedContent != null; }}
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
                if (this.IsLink)
                    return "application/xml";
                else
                    return "application/atom+xml";
            }
        }

        public string[] Accept
        {
            get
            {
                if (this.Method == RestVerbs.GET && !this.ReturnsScalarResult || this.ReturnContent)
                    return new[] {"application/text", "application/xml", "application/atom+xml"};
                else
                    return null;
            }
        }

        public HttpContent GetContent()
        {
            return this.FormattedContent != null
                ? new StringContent(this.FormattedContent, Encoding.UTF8, this.ContentType)
                : null;
        }

        public bool IsLink { get; set; }
        public bool ReturnsScalarResult { get; set; }
        public bool ReturnContent { get; set; }
        public bool CheckOptimisticConcurrency { get; set; }

        public object Message { get; set; }

        internal ODataRequest(string method, Session session, string commandText)
        {
            this.Method = method;
            this.Uri = CreateRequestUrl(session, commandText);
            this.CommandText = commandText;
            this.Credentials = session.Credentials;
        }

        internal ODataRequest(string method, Session session, string commandText, HttpRequestMessage requestMessage)
            : this(method, session, commandText)
        {
            this.RequestMessage = requestMessage;
        }

        internal ODataRequest(string method, Session session, string commandText, IDictionary<string, object> entryData, string formattedContent)
            : this(method, session, commandText)
        {
            EntryData = entryData;
            FormattedContent = formattedContent;
        }

        private string CreateRequestUrl(Session session, string commandText)
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

            if (this.HasContent)
            {
                _requestMessage.Content = this.GetContent();
            }
            return _requestMessage;
        }
    }
}