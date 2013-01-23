using System;
using System.Net;
using System.Text;
using Simple.NExtLib;

namespace Simple.OData.Client
{
    class CommandRequestBuilder : RequestBuilder
    {
#if (NET20 || NET35 || NET40 || SILVERLIGHT)
        public CommandRequestBuilder(string urlBase, Credentials credentials)
            : base(urlBase, credentials)
#else
        public CommandRequestBuilder(string urlBase)
            : base(urlBase)
#endif
        {
        }

        public override void AddCommandToRequest(HttpCommand command)
        {
            var uri = CreateRequestUrl(command.CommandText);
            var request = CreateWebRequest(uri);

            request.Method = command.Method;

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
