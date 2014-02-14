using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Simple.OData.Client
{
    public class ODataBatch : IDisposable
    {
        private bool _active;
        internal ODataClientSettings Settings { get; set; }
        internal BatchRequestBuilder RequestBuilder { get; set; }
        internal BatchRequestRunner RequestRunner { get; set; }

        public ODataBatch(string urlBase)
            : this (new ODataClientSettings { UrlBase = urlBase })
        {
        }

        public ODataBatch(ODataClientSettings settings)
        {
            this.Settings = settings;
            this.RequestBuilder = new BatchRequestBuilder(this.Settings.UrlBase, this.Settings.Credentials);
            this.RequestRunner = new BatchRequestRunner(this.RequestBuilder);

            this.RequestBuilder.BeginBatch();
            _active = true;
        }

        public void Dispose()
        {
            if (_active)
                this.RequestBuilder.CancelBatch();
            _active = false;
        }

        public void Complete()
        {
            this.RequestBuilder.EndBatch();
            using (var stream = this.RequestRunner.ExecuteRequest(this.RequestBuilder.Request).GetResponseStream())
            {
                ParseResponse(stream);
            }
            _active = false;
        }

        public void Cancel()
        {
            this.RequestBuilder.CancelBatch();
            _active = false;
        }

        private void ParseResponse(Stream stream)
        {
            var content = Utils.StreamToString(stream);
            var batchMarker = Regex.Match(content, @"--batchresponse_[a-zA-Z0-9\-]+").Value;
            var batchResponse = content.Split(new string[] { batchMarker }, StringSplitOptions.None)[1];
            var changesetMarker = Regex.Match(batchResponse, @"--changesetresponse_[a-zA-Z0-9\-]+").Value;
            var changesetResponses = batchResponse.Split(new string[] { changesetMarker }, StringSplitOptions.None).ToList();
            changesetResponses = changesetResponses.Skip(1).Take(changesetResponses.Count - 2).ToList();
            foreach (var changesetResponse in changesetResponses)
            {
                var match = Regex.Match(changesetResponse, @"HTTP/[0-9\.]+\s+([0-9]+)\s+(.+)\n");
                var statusCode = int.Parse(match.Groups[1].Value);
                var message = match.Groups[2].Value;
                if (statusCode >= 400)
                {
                    throw new WebRequestException(message, statusCode.ToString());
                }
            }
        }
    }
}
