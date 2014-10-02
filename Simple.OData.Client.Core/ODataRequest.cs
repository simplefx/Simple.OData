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
        private readonly string _uri;
        private HttpRequestMessage _requestMessage;
        private readonly ODataPayloadFormat _payloadFormat;
        private readonly Stream _contentStream;

        public HttpRequestMessage RequestMessage
        {
            get { return GetOrCreateRequestMessage(); }
            private set { _requestMessage = value; }
        }

        public string[] Accept
        {
            get
            {
                switch (this._payloadFormat)
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

        public string Method { get; private set; }
        public ICredentials Credentials { get; private set; }
        public IDictionary<string, object> EntryData { get; private set; }
        public bool IsLink { get; set; }
        public bool ReturnsScalarResult { get; set; }
        public bool ReturnContent { get; set; }
        public bool CheckOptimisticConcurrency { get; set; }

        internal ODataRequest(string method, ISession session, string commandText)
        {
            this.Method = method;
            this.Credentials = session.Credentials;

            _uri = Utils.CreateAbsoluteUri(session.UrlBase, commandText).AbsoluteUri;
            _payloadFormat = session.PayloadFormat;
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
            _contentStream = contentStream;
        }

        private HttpContent GetContent()
        {
            return this._contentStream != null
                ? new StringContent(Utils.StreamToString(this._contentStream), Encoding.UTF8, this.GetContentType())
                : null;
        }

        private string GetContentType()
        {
            switch (this._payloadFormat)
            {
                default:
                case ODataPayloadFormat.Atom:
                    return this.IsLink ? "application/xml" : "application/atom+xml";

                case ODataPayloadFormat.Json:
                    return "application/json";
            }
        }

        private HttpRequestMessage GetOrCreateRequestMessage()
        {
            if (_requestMessage != null)
                return _requestMessage;

            _requestMessage = new HttpRequestMessage(new HttpMethod(this.Method), this._uri)
            {
                Content = this._contentStream != null ? this.GetContent() : null
            };
            return _requestMessage;
        }
    }
}