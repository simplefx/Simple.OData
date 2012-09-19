using System.Collections.Generic;
using System.Net;

namespace Simple.Data.OData
{
    public class HttpCommand
    {
        public string Method { get; set; }
        public string CommandText { get; set; }
        public IDictionary<string, object> OriginalContent { get; set; }
        public string FormattedContent { get; set; }
        public HttpWebRequest Request { get; set; }
        public int ContentId { get; set; }

        private HttpCommand()
        {
        }

        public HttpCommand(string method, string commandText, IDictionary<string, object> originalContent = null, string formattedContent = null)
        {
            Method = method;
            CommandText = commandText;
            OriginalContent = originalContent;
            FormattedContent = formattedContent;
        }

        public static HttpCommand Get(string commandText)
        {
            return new HttpCommand
                {
                    CommandText = commandText, Method = RestVerbs.GET
                };
        }

        public static HttpCommand Post(string commandText, IDictionary<string, object> originalContent, string formattedContent)
        {
            return new HttpCommand
                {
                    Method = RestVerbs.POST,
                    CommandText = commandText,
                    OriginalContent = originalContent,
                    FormattedContent = formattedContent
                };
        }

        public static HttpCommand Put(string commandText, IDictionary<string, object> originalContent, string formattedContent)
        {
            return new HttpCommand
                {
                    Method = RestVerbs.PUT,
                    CommandText = commandText,
                    OriginalContent = originalContent,
                    FormattedContent = formattedContent
                };
        }

        public static HttpCommand Merge(string commandText, IDictionary<string, object> originalContent, string formattedContent)
        {
            return new HttpCommand
                {
                    Method = RestVerbs.MERGE,
                    CommandText = commandText,
                    OriginalContent = originalContent,
                    FormattedContent = formattedContent
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
