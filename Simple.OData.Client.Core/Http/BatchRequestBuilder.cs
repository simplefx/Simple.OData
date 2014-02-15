using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Simple.OData.Client
{
    internal class BatchRequestBuilder : RequestBuilder
    {
        private string _batchId;
        private string _changesetId;
        private int _contentId;
        private MultipartContent _content;

        public HttpRequest Request { get; private set; }

        public BatchRequestBuilder(string urlBase, ICredentials credentials = null)
            : base(urlBase, credentials)
        {
        }

        public void BeginBatch()
        {
            _batchId = Guid.NewGuid().ToString();
            _changesetId = Guid.NewGuid().ToString();

            this.Request = CreateRequest(CreateRequestUrl(FluentCommand.BatchLiteral));
            this.Request.Method = RestVerbs.POST;
            var batchContent = new MultipartContent("mixed", "batch_" + _batchId);
            this.Request.Content = batchContent;
            this.Request.ContentType = "application/http";
            var changesetContent = new MultipartContent("mixed", "changeset_" + _changesetId);
            batchContent.Add(changesetContent);
            _content = changesetContent;
        }

        public void EndBatch()
        {
            _content = null;
        }

        public void CancelBatch()
        {
            _content = null;
        }

        public override HttpRequest CreateRequest(HttpCommand command)
        {
            var request = new CommandRequestBuilder(this.UrlBase, this.Credentials).CreateRequest(command);
            var content = new StringContent(FormatBatchItem(command));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/http");
            content.Headers.Add("Content-Transfer-Encoding", "binary");

            var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method), request.Uri);
            requestMessage.Content = content;
            if (requestMessage.Content != null)
            {
                _content.Add(requestMessage.Content);
            }

            request.EntryData = command.EntryData;
            if (request.EntryData != null)
            {
                request.EntryData.Add("$Batch-ID", _batchId);
                request.EntryData.Add("$Content-ID", _contentId);
            }
            command.ContentId = _contentId;

            return request;
        }

        public override int GetContentId(object content)
        {
            var properties = content as IDictionary<string, object>;
            if (properties != null)
            {
                object val;
                if (properties.TryGetValue("$Batch-ID", out val) && val.ToString() == _batchId)
                {
                    return properties.TryGetValue("$Content-ID", out val) ? int.Parse(val.ToString()) : 0;
                }
            }
            return 0;
        }

        private string FormatBatchItem(HttpCommand command)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0} {1} HTTP/{2}",
                command.Method, command.IsLink ? command.CommandText : CreateRequestUrl(command.CommandText), "1.1"));

            if (command.FormattedContent != null)
            {
                sb.AppendLine(string.Format("Content-ID: {0}", ++_contentId));
                sb.AppendLine(string.Format("Content-Type: {0}", command.ContentType));
                sb.AppendLine(string.Format("Content-Length: {0}", (command.FormattedContent ?? string.Empty).Length));
                sb.AppendLine();
                sb.Append(command.FormattedContent);
            }

            return sb.ToString();
        }
    }
}
