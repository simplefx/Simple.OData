using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    /// <summary>
    /// Performs batch processing of OData requests by grouping multiple operations in a single HTTP POST request in accordance with OData protocol
    /// </summary>
    public class ODataBatch
    {
        private readonly ODataClient _client;
        private readonly List<Func<IODataClient, Task>> _actions = new List<Func<IODataClient, Task>>();

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
            _client = new ODataClient(settings, true);
        }

        public static ODataBatch operator +(ODataBatch batch, Func<IODataClient, Task> action)
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
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _client.ExecuteBatchAsync(_actions, cancellationToken);
        }
    }
}
