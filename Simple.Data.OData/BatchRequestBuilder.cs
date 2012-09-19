using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Simple.NExtLib;

namespace Simple.Data.OData
{
    public class BatchRequestBuilder : RequestBuilder
    {
        private string _batchId;
        private string _changesetId;
        private int _contentId;
        private StringBuilder _contentBuilder;
        private Dictionary<object, HttpCommand> _commandContents;

        public HttpWebRequest Request { get; private set; }

        public BatchRequestBuilder(string urlBase)
            : base(urlBase)
        {
        }

        public void BeginBatch()
        {
            var uri = CreateRequestUrl("$batch");
            this.Request = (HttpWebRequest)WebRequest.Create(uri);
            this.Request.Method = RestVerbs.POST;
            _batchId = Guid.NewGuid().ToString();
            this.Request.ContentType = string.Format("multipart/mixed; boundary=batch_{0}", _batchId);

            _contentBuilder = new StringBuilder();
            _contentBuilder.AppendLine();
            _contentBuilder.AppendLine(string.Format("--batch_{0}", _batchId));

            _changesetId = Guid.NewGuid().ToString();
            _contentBuilder.AppendLine(string.Format("Content-Type: multipart/mixed; boundary=changeset_{0}", _changesetId));
            _contentBuilder.AppendLine();

            _commandContents = new Dictionary<object, HttpCommand>();
        }

        public void EndBatch()
        {
            _contentBuilder.AppendLine(string.Format("--changeset_{0}--", _changesetId));
            _contentBuilder.AppendLine(string.Format("--batch_{0}--", _batchId));
            var content = this._contentBuilder.ToString();
            this.Request.ContentLength = content.Length;
            this.Request.SetContent(content);
            _contentBuilder.Clear();
            _commandContents.Clear();
        }

        public void CancelBatch()
        {
            _contentBuilder.Clear();
            _commandContents.Clear();
        }

        public override void AddCommandToRequest(HttpCommand command)
        {
            _contentBuilder.AppendLine(string.Format("--changeset_{0}", _changesetId));
            _contentBuilder.AppendLine("Content-Type: application/http");
            _contentBuilder.AppendLine("Content-Transfer-Encoding:binary");
            _contentBuilder.AppendLine();

            _contentBuilder.AppendLine(string.Format("{0} {1} HTTP/{2}", command.Method, CreateRequestUrl(command.CommandText), "1.1"));

            if (command.FormattedContent != null)
            {
                _contentBuilder.AppendLine(string.Format("Content-ID: {0}", ++_contentId));
                _contentBuilder.AppendLine(string.Format("Content-Type: application/atom+xml;type=entry"));
                _contentBuilder.AppendLine(string.Format("Content-Length: {0}", (command.FormattedContent ?? string.Empty).Length));
                _contentBuilder.AppendLine();
                _contentBuilder.Append(command.FormattedContent);
            }

            _contentBuilder.AppendLine();

            command.Request = this.Request;
            command.ContentId = _contentId;

            if (command.OriginalContent != null)
            {
                _commandContents.Add(command.OriginalContent, command);
            }
        }

        public override HttpCommand GetContentCommand(object content)
        {
            HttpCommand command = null;
            _commandContents.TryGetValue(content, out command);
            return command;
        }
    }
}