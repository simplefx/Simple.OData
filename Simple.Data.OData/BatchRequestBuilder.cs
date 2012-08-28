using System;
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
        }

        public void EndBatch()
        {
            _contentBuilder.AppendLine(string.Format("--changeset_{0}--", _changesetId));
            _contentBuilder.AppendLine(string.Format("--batch_{0}--", _batchId));
            var content = this._contentBuilder.ToString();
            this.Request.ContentLength = content.Length;
            this.Request.SetContent(content);
            _contentBuilder.Clear();
        }

        public void CancelBatch()
        {
            _contentBuilder.Clear();
        }

        public override void AddTableCommand(string command, string method, string content = null)
        {
            _contentBuilder.AppendLine(string.Format("--changeset_{0}", _changesetId));
            _contentBuilder.AppendLine("Content-Type: application/http");
            _contentBuilder.AppendLine("Content-Transfer-Encoding:binary");
            _contentBuilder.AppendLine();

            _contentBuilder.AppendLine(string.Format("{0} {1} HTTP/{2}", method, CreateRequestUrl(command), "1.1"));

            if (content != null)
            {
                _contentBuilder.AppendLine(string.Format("Content-ID: {0}", ++_contentId));
                _contentBuilder.AppendLine(string.Format("Content-Type: application/atom+xml;type=entry"));
                _contentBuilder.AppendLine(string.Format("Content-Length: {0}", (content ?? string.Empty).Length));
                _contentBuilder.AppendLine();
                _contentBuilder.Append(content);
            }

            _contentBuilder.AppendLine();
        }
    }
}