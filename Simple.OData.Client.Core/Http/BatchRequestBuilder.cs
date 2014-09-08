using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    class BatchRequestBuilder : RequestBuilder
    {
        private readonly Lazy<IBatchWriter> _lazyBatchWriter;
        private string _batchId;
        private string _changesetId;
        private int _contentId;
        //private MultipartContent _content;
        private HttpRequestMessage _request;

        public BatchRequestBuilder(ISession session)
            : base(session)
        {
            _lazyBatchWriter = new Lazy<IBatchWriter>(() => this.Session.Provider.GetBatchWriter());
        }

        //public void BeginBatch()
        //{
        //    //_lazyBatchWriter.Value.IsActive = true;

        //    //_batchId = Guid.NewGuid().ToString();
        //    //_changesetId = Guid.NewGuid().ToString();

        //    //this.Request = CreateRequest(CreateRequestUrl(FluentCommand.BatchLiteral));
        //    //this.Request.Method = RestVerbs.POST;
        //    //var batchContent = new MultipartContent("mixed", "batch_" + _batchId);
        //    //this.Request.Content = batchContent;
        //    //this.Request.ContentType = "application/http";
        //    //var changesetContent = new MultipartContent("mixed", "changeset_" + _changesetId);
        //    //batchContent.Add(changesetContent);
        //    //_content = changesetContent;
        //}

        public async Task<HttpRequestMessage> CompleteBatchAsync()
        {
            _request = await _lazyBatchWriter.Value.EndBatchAsync();
            return _request;
            //_content = null;
        }

        public override HttpRequest CreateRequest(HttpCommand command, bool returnContent = false, bool checkOptimisticConcurrency = false)
        {
            var request = new CommandRequestBuilder(this.Session).CreateRequest(command);
            //var content = new StringContent(FormatBatchItem(command, checkOptimisticConcurrency));
            //content.Headers.ContentType = new MediaTypeHeaderValue("application/http");
            //content.Headers.Add("Content-Transfer-Encoding", "binary");
            //var content = command.FormattedContent;

            //var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method), request.Uri);
            //requestMessage.Content = content;
            //if (requestMessage.Content != null)
            //{
            //    _content.Add(requestMessage.Content);
            //}

            //request.EntryData = command.EntryData;
            //if (request.EntryData != null)
            //{
            //    request.EntryData.Add("$Batch-ID", _batchId);
            //    request.EntryData.Add("$Content-ID", _contentId);
            //}
            //command.ContentId = _contentId;

            return request;
        }

        public HttpRequest CreateBatchRequest()
        {
            var request = CreateRequest(CreateRequestUrl(FluentCommand.BatchLiteral));
            request.Method = RestVerbs.POST;
            //var batchContent = new MultipartContent("mixed", "batch_" + _batchId);
            //request.Content = batchContent;
            //request.Content = new StringContent(_content);
            //request.ContentType = "application/http";
            return request;
        }

        public override int GetBatchContentId(object content)
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

        public override Lazy<IBatchWriter> GetDeferredBatchWriter()
        {
            return _lazyBatchWriter;
        }

        //private string FormatBatchItem(HttpCommand command, bool checkOptimisticConcurrency)
        //{
        //    var sb = new StringBuilder();
        //    sb.AppendLine(string.Format("{0} {1} HTTP/{2}",
        //        command.Method, command.IsLink ? command.CommandText : CreateRequestUrl(command.CommandText), "1.1"));

        //    if (command.FormattedContent != null)
        //    {
        //        sb.AppendLine(string.Format("Content-ID: {0}", ++_contentId));
        //        sb.AppendLine(string.Format("Content-Type: {0}", command.ContentType));
        //        sb.AppendLine(string.Format("Content-Length: {0}", (command.FormattedContent ?? string.Empty).Length));
        //        if (checkOptimisticConcurrency)
        //        {
        //            sb.AppendLine(string.Format("If-Match: {0}", EntityTagHeaderValue.Any.Tag));
        //        }
        //        sb.AppendLine();
        //        sb.Append(command.FormattedContent);
        //    }
        //    else if (checkOptimisticConcurrency)
        //    {
        //        sb.AppendLine(string.Format("If-Match: {0}", EntityTagHeaderValue.Any.Tag));
        //    }

        //    return sb.ToString();
        //}
    }
}
