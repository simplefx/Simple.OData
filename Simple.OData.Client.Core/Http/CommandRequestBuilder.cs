using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Simple.OData.Client
{
    class CommandRequestBuilder : RequestBuilder
    {
        public CommandRequestBuilder(string urlBase, ICredentials credentials)
            : base(urlBase, credentials)
        {
        }

        public override HttpRequest CreateRequest(HttpCommand command, bool returnContent = false, bool checkOptimisticConcurrency = false)
        {
            var uri = CreateRequestUrl(command.CommandText);
            var request = CreateRequest(uri);
            request.Method = command.Method;
            request.ReturnContent = returnContent;
            request.CheckOptimisticConcurrency = checkOptimisticConcurrency;

            if (command.FormattedContent != null)
            {
                request.ContentType = command.ContentType;
                request.Content = new StringContent(command.FormattedContent, Encoding.UTF8, command.ContentType);
            }

            if (request.Method == RestVerbs.GET && !command.ReturnsScalarResult || request.ReturnContent)
            {
                request.Accept = new[] { "application/text", "application/xml", "application/atom+xml" };
            }

            return request;
        }

        public override int GetContentId(object content)
        {
            return 0;
        }
    }
}
