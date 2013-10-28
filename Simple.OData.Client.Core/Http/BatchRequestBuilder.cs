using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class BatchRequestBuilder : RequestBuilder
    {
        private string _batchId;
        private string _changesetId;
        private int _contentId;
        private StringBuilder _contentBuilder;

        public HttpWebRequest Request { get; private set; }

        public BatchRequestBuilder(string urlBase, ICredentials credentials = null)
            : base(urlBase, credentials)
        {
        }

        public void BeginBatch()
        {
            var uri = CreateRequestUrl(ODataCommand.BatchLiteral);
            this.Request = CreateWebRequest(uri);
            this.Request.Method = RestVerbs.POST;
            _batchId = Guid.NewGuid().ToString();
            this.Request.ContentType = string.Format("multipart/mixed; boundary=batch_{0}", _batchId);

            _contentBuilder = new StringBuilder();
            _contentBuilder.AppendLine();
            _contentBuilder.AppendLine(string.Format("--batch_{0}", _batchId));

            _changesetId = Guid.NewGuid().ToString();
            _contentBuilder.AppendLine(string.Format("Content-Type: multipart/mixed; boundary=changeset_{0}", _changesetId));
            _contentBuilder.AppendLine();
        }

        public void EndBatch()
        {
            _contentBuilder.AppendLine(string.Format("--changeset_{0}--", _changesetId));
            _contentBuilder.AppendLine(string.Format("--batch_{0}--", _batchId));
            var content = this._contentBuilder.ToString();
            this.Request.SetContent(content);
            _contentBuilder.Clear();
        }

        public void CancelBatch()
        {
            _contentBuilder.Clear();
        }

        public override void AddCommandToRequest(HttpCommand command)
        {
            _contentBuilder.AppendLine(string.Format("--changeset_{0}", _changesetId));
            _contentBuilder.AppendLine("Content-Type: application/http");
            _contentBuilder.AppendLine("Content-Transfer-Encoding:binary");
            _contentBuilder.AppendLine();

            _contentBuilder.AppendLine(string.Format("{0} {1} HTTP/{2}", command.Method, command.IsLink ? command.CommandText : CreateRequestUrl(command.CommandText), "1.1"));

            if (command.FormattedContent != null)
            {
                _contentBuilder.AppendLine(string.Format("Content-ID: {0}", ++_contentId));
                _contentBuilder.AppendLine(string.Format("Content-Type: {0}", command.ContentType));
                _contentBuilder.AppendLine(string.Format("Content-Length: {0}", (command.FormattedContent ?? string.Empty).Length));
                _contentBuilder.AppendLine();
                _contentBuilder.Append(command.FormattedContent);
            }

            _contentBuilder.AppendLine();

            command.Request = this.Request;
            command.ContentId = _contentId;

            if (command.OriginalContent != null)
            {
                command.OriginalContent.Add("$Batch-ID", _batchId);
                command.OriginalContent.Add("$Content-ID", command.ContentId);
            }
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
    }
}
