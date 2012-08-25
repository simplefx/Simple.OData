using System;
using System.Net;
using System.Text;
using Simple.NExtLib;
using Simple.OData;

namespace Simple.Data.OData
{
    public class BatchRequestBuilder : RequestBuilder
    {
        private string _batchId;
        private string _changesetId;
        private StringBuilder _contentBuilder;

        public BatchRequestBuilder(string urlBase)
            : base(urlBase)
        {
        }

        public void BeginBatch(string command, string method, string content = null)
        {
            var uri = CreateRequestUrl(command);
            this.Request = (HttpWebRequest)WebRequest.Create(uri);
            this.Request.Method = RestVerbs.POST;
            _batchId = Guid.NewGuid().ToString();
            this.Request.ContentType = "multipart/mixed; boundary=" + _batchId;
            _contentBuilder = new StringBuilder();
        }

        public void EndBatch()
        {
            _contentBuilder.AppendLine(string.Format("--batch({0})--", _batchId));
            AddContent(this.Request, _contentBuilder.ToString());
        }

        public override void AddTableCommand(string command, string method, string content = null)
        {
            _contentBuilder.AppendLine(string.Format("--batch_{0}", _batchId));
            if (method == RestVerbs.GET)
            {
                if (_changesetId != null)
                {
                    _contentBuilder.AppendLine(string.Format("--changeset_{0}--", _changesetId));
                    _contentBuilder.AppendLine();
                }
                _changesetId = null;
            }
            else
            {
                if (_changesetId == null)
                {
                    _changesetId = Guid.NewGuid().ToString();
                    _contentBuilder.AppendLine(string.Format("Content-Type: multipart/mixed; boundary=changeset_{0}", _changesetId));
                    _contentBuilder.AppendLine(string.Format("Content-Length: {0}", 0));
                    _contentBuilder.AppendLine();
                    _contentBuilder.AppendLine(string.Format("--changeset_{0}", _changesetId));
                }
            }
            _contentBuilder.AppendLine("Content-Type: application/http");
            _contentBuilder.AppendLine("Content-Transfer-Encoding:binary");
            _contentBuilder.AppendLine();
            _contentBuilder.AppendLine(string.Format("{0} {1} HTTP/{2}", method, command, "1.1"));
            _contentBuilder.AppendLine(this.Host);

            if (content != null)
            {
                _contentBuilder.AppendLine(string.Format("Content-Type: application/atom+xml;type=entry"));
                _contentBuilder.AppendLine(string.Format("Content-Length: {0}", (content ?? string.Empty).Length));
                _contentBuilder.Append(content);
            }
        }

        protected override void AddContent(WebRequest request, string content)
        {
            _contentBuilder.AppendLine(content);
        }
    }
}