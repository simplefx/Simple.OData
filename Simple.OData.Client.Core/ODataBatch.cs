using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            this.RequestRunner = new BatchRequestRunner();

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
            try
            {
                CompleteAsync().Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        public async Task CompleteAsync()
        {
            this.RequestBuilder.EndBatch();
            using (var response = await this.RequestRunner.ExecuteRequestAsync(this.RequestBuilder.Request))
            {
                await ParseResponseAsync(response);
            }
            _active = false;
        }

        public void Cancel()
        {
            this.RequestBuilder.CancelBatch();
            _active = false;
        }

        private async Task ParseResponseAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var batchMarker = Regex.Match(content, @"--batchresponse_[a-zA-Z0-9\-]+").Value;
            var batchResponse = content.Split(new string[] {batchMarker}, StringSplitOptions.None)[1];
            var changesetMarker = Regex.Match(batchResponse, @"--changesetresponse_[a-zA-Z0-9\-]+").Value;
            var changesetResponses =
                batchResponse.Split(new string[] {changesetMarker}, StringSplitOptions.None).ToList();
            changesetResponses = changesetResponses.Skip(1).Take(changesetResponses.Count - 2).ToList();
            foreach (var changesetResponse in changesetResponses)
            {
                var match = Regex.Match(changesetResponse, @"HTTP/[0-9\.]+\s+([0-9]+)\s+(.+)\n");
                var statusCode = int.Parse(match.Groups[1].Value);
                var message = match.Groups[2].Value;
                if (statusCode >= 400)
                {
                    throw new WebRequestException(message, (HttpStatusCode)statusCode);
                }
            }
        }
    }
}
