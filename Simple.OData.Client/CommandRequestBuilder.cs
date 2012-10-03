using System.Collections.Generic;
using System.Net;
using Simple.NExtLib;

namespace Simple.OData.Client
{
    class CommandRequestBuilder : RequestBuilder
    {
        public CommandRequestBuilder(string urlBase)
            : base(urlBase)
        {
        }

        public override void AddCommandToRequest(HttpCommand command)
        {
            var uri = CreateRequestUrl(command.CommandText);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = command.Method;
#if !NETFX_CORE
            request.ContentLength = (command.FormattedContent ?? string.Empty).Length;
#endif

            // TODO: revise
            //if (method == "PUT" || method == "DELETE" || method == "MERGE")
            //{
            //    request.Headers.Add("If-Match", "*");
            //}

            if (command.FormattedContent != null)
            {
                request.ContentType = command.ContentType;
                request.SetContent(command.FormattedContent);
            }

            command.Request = request;
        }

        public override int GetContentId(object content)
        {
            return 0;
        }
    }
}