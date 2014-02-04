using System.Collections.Generic;
using System.Net;

namespace Simple.OData.Client
{
    public class HttpCommand
    {
        public string Method { get; set; }
        public string CommandText { get; set; }
        public IDictionary<string, object> OriginalContent { get; set; }
        public string FormattedContent { get; set; }
        public bool IsLink { get; set; }
        public bool ReturnsScalarResult { get; set; }
        public HttpWebRequest Request { get; set; }
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

        private HttpCommand()
        {
        }

        public HttpCommand(string method, string commandText)
            : this(method, commandText, null, null, false)
        {
        }

        public HttpCommand(string method, string commandText, IDictionary<string, object> originalContent, string formattedContent, bool isLink = false)
        {
            Method = method;
            CommandText = commandText;
            OriginalContent = originalContent;
            FormattedContent = formattedContent;
            IsLink = isLink;
        }

        public static HttpCommand Get(string commandText, bool scalarResult = false)
        {
            return new HttpCommand
                {
                    CommandText = commandText, Method = RestVerbs.GET, ReturnsScalarResult = scalarResult
                };
        }

        public static HttpCommand Post(string commandText, IDictionary<string, object> originalContent, string formattedContent, bool isLink = false)
        {
            return new HttpCommand
                {
                    Method = RestVerbs.POST,
                    CommandText = commandText,
                    OriginalContent = originalContent,
                    FormattedContent = formattedContent,
                    IsLink = isLink
                };
        }

        public static HttpCommand Put(string commandText, IDictionary<string, object> originalContent, string formattedContent, bool isLink = false)
        {
            return new HttpCommand
                {
                    Method = RestVerbs.PUT,
                    CommandText = commandText,
                    OriginalContent = originalContent,
                    FormattedContent = formattedContent,
                    IsLink = isLink
                };
        }

        public static HttpCommand Merge(string commandText, IDictionary<string, object> originalContent, string formattedContent, bool isLink = false)
        {
            return new HttpCommand
                {
                    Method = RestVerbs.MERGE,
                    CommandText = commandText,
                    OriginalContent = originalContent,
                    FormattedContent = formattedContent,
                    IsLink = isLink
                };
        }

        public static HttpCommand Delete(string commandText)
        {
            return new HttpCommand
                {
                    Method = RestVerbs.DELETE,
                    CommandText = commandText 
                };
        }
    }
}
