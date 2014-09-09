using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Simple.OData.Client
{
    class ODataRequest
    {
        public string Uri { get; set; }
        public ICredentials Credentials { get; set; }
        public string Method { get; set; }
        public string CommandText { get; set; }
        public IDictionary<string, object> EntryData { get; set; }
        public HttpContent Content { get; set; }
        public string FormattedContent { get; set; }
        public int ContentId { get; set; }

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

        internal ODataRequest(string method, Session session, string commandText, IDictionary<string, object> entryData, string formattedContent)
        {
            Method = method;
            Uri = CreateRequestUrl(session, commandText);
            CommandText = commandText;
            Credentials = session.Credentials;
            EntryData = entryData;
            FormattedContent = formattedContent;

            if (this.FormattedContent != null)
            {
                this.Content = new StringContent(this.FormattedContent, Encoding.UTF8, this.ContentType);
            }
        }

        //public static ODataRequest Get(Session session, string commandText, bool scalarResult = false)
        //{
        //    return new ODataRequest(session, commandText)
        //        {
        //            Method = RestVerbs.GET, 
        //            ReturnsScalarResult = scalarResult,
        //            ReturnContent = false,
        //            CheckOptimisticConcurrency = false,
        //        }.Enrich();
        //}

        //public static ODataRequest Post(Session session, string commandText, IDictionary<string, object> entryData, string formattedContent, bool isLink, bool resultRequired)
        //{
        //    return new ODataRequest(session, commandText)
        //    {
        //            Method = RestVerbs.POST,
        //            EntryData = entryData,
        //            FormattedContent = formattedContent,
        //            IsLink = isLink,
        //            ReturnContent = resultRequired,
        //            CheckOptimisticConcurrency = false,
        //    }.Enrich();
        //}

        //public static ODataRequest Put(Session session, string commandText, IDictionary<string, object> entryData, string formattedContent, bool isLink, bool resultRequired, bool checkOptimisticConcurrency)
        //{
        //    return new ODataRequest(session, commandText)
        //    {
        //            Method = RestVerbs.PUT,
        //            EntryData = entryData,
        //            FormattedContent = formattedContent,
        //            IsLink = isLink,
        //            ReturnContent = resultRequired,
        //            CheckOptimisticConcurrency = checkOptimisticConcurrency,
        //    }.Enrich();
        //}

        //public static ODataRequest Merge(Session session, string commandText, IDictionary<string, object> entryData, string formattedContent, bool isLink, bool resultRequired, bool checkOptimisticConcurrency)
        //{
        //    return new ODataRequest(session, commandText)
        //    {
        //            Method = RestVerbs.MERGE,
        //            EntryData = entryData,
        //            FormattedContent = formattedContent,
        //            IsLink = isLink,
        //            ReturnContent = resultRequired,
        //            CheckOptimisticConcurrency = checkOptimisticConcurrency,
        //    }.Enrich();
        //}

        //public static ODataRequest Delete(Session session, string commandText, bool checkOptimisticConcurrency)
        //{
        //    return new ODataRequest(session, commandText)
        //    {
        //            Method = RestVerbs.DELETE,
        //            CheckOptimisticConcurrency = checkOptimisticConcurrency,
        //    }.Enrich();
        //}

        private string CreateRequestUrl(Session session, string commandText)
        {
            string url = string.IsNullOrEmpty(session.UrlBase) ? "http://" : session.UrlBase;
            if (!url.EndsWith("/"))
                url += "/";
            return url + commandText;
        }
    }
}