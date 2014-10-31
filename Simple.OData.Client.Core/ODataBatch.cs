using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    /// <summary>
    /// Performs batch processing of OData requests by grouping multiple operations in a single HTTP POST request in accordance with OData protocol
    /// </summary>
    public class ODataBatch
    {
        internal Session Session { get; private set; }
        internal RequestBuilder RequestBuilder { get; private set; }
        internal RequestRunner RequestRunner { get; private set; }

        private readonly List<Action<IODataClient>> _actions = new List<Action<IODataClient>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatch"/> class.
        /// </summary>
        /// <param name="urlBase">The URL base.</param>
        public ODataBatch(string urlBase)
            : this(new ODataClientSettings { UrlBase = urlBase })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatch"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public ODataBatch(ODataClientSettings settings)
        {
            this.Session = Session.FromSettings(settings);
            this.RequestBuilder = new RequestBuilder(this.Session, true);
            this.RequestRunner = new RequestRunner(this.Session);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatch"/> class.
        /// </summary>
        /// <param name="session">The OData client session.</param>
        internal ODataBatch(Session session)
        {
            this.Session = session;
            this.RequestBuilder = new RequestBuilder(this.Session, true);
            this.RequestRunner = new RequestRunner(this.Session);
        }

        public static ODataBatch operator +(ODataBatch batch, Action<IODataClient> action)
        {
            batch._actions.Add(action);
            return batch;
        }

        /// <summary>
        /// Executes the OData batch by submitting pending requests to the OData service.
        /// </summary>
        /// <returns></returns>
        public Task ExecuteAsync()
        {
            return ExecuteAsync(CancellationToken.None);
        }

        /// <summary>
        /// Executes the OData batch by submitting pending requests to the OData service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await this.Session.ResolveAdapterAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            if (_actions.Any())
            {
                var client = new ODataClient(this);
                foreach (var action in _actions)
                {
                    action(client);
                }

                ODataResponse batchResponse;
                using (var response = await this.RequestRunner.ExecuteRequestAsync(
                    await this.RequestBuilder.CreateBatchRequestAsync(), cancellationToken))
                {
                    var responseReader = this.Session.Adapter.GetResponseReader();
                    batchResponse = await responseReader.GetResponseAsync(response, this.Session.Settings.IncludeResourceTypeInEntryProperties);
                }

                for (int actionIndex = 0; actionIndex < _actions.Count && actionIndex < batchResponse.Batch.Count; actionIndex++)
                {
                    var actionResponse = batchResponse.Batch[actionIndex];
                    if (actionResponse.StatusCode >= 400)
                    {
                        var statusCode = (HttpStatusCode) actionResponse.StatusCode;
                        throw new WebRequestException(statusCode.ToString(), statusCode);
                    }

                    client = new ODataClient(actionResponse);
                    _actions[actionIndex](client);
                }
            }
        }
    }
}
