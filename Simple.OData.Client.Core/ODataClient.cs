using System;
using System.Collections.Generic;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    /// <summary>
    /// Provides access to OData operations.
    /// </summary>
    public partial class ODataClient : IODataClient
    {
        private readonly ODataClientSettings _settings;
        private readonly Session _session;
        private readonly RequestBuilder _requestBuilder;
        private readonly RequestRunner _requestRunner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataClient"/> class.
        /// </summary>
        /// <param name="urlBase">The OData service URL.</param>
        public ODataClient(string urlBase)
            : this(new ODataClientSettings {UrlBase = urlBase})
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataClient"/> class.
        /// </summary>
        /// <param name="settings">The OData client settings.</param>
        public ODataClient(ODataClientSettings settings)
        {
            _settings = settings;
            _session = Session.FromSettings(_settings);

            _requestBuilder = new RequestBuilder(_session);
            _requestRunner = new RequestRunner(_session);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataClient"/> class.
        /// </summary>
        /// <param name="batch">The OData batch instance.</param>
        public ODataClient(ODataBatch batch)
        {
            _settings = batch.Settings;
            _session = Session.FromSettings(_settings);

            _requestBuilder = batch.RequestBuilder;
            _requestRunner = batch.RequestRunner;
        }

        internal Session Session
        {
            get { return _session; }
        }

        /// <summary>
        /// Parses the OData service metadata string.
        /// </summary>
        /// <typeparam name="T">OData protocol specific metadata interface</typeparam>
        /// <param name="metadataString">The metadata string.</param>
        /// <returns>
        /// The service metadata.
        /// </returns>
        public static T ParseMetadataString<T>(string metadataString)
        {
            var session = Session.FromMetadata("http://localhost/" + metadataString.GetHashCode() + "$metadata", metadataString);
            return (T)session.Adapter.Model;
        }

        /// <summary>
        /// Returns an instance of a fluent OData client for the specified collection.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns>
        /// The fluent OData client instance.
        /// </returns>
        public IFluentClient<IDictionary<string, object>> For(string collectionName)
        {
            return GetFluentClient().For(collectionName);
        }

        /// <summary>
        /// Returns an instance of a fluent OData client for the specified collection.
        /// </summary>
        /// <param name="expression">Collection expression.</param>
        /// <returns>
        /// The fluent OData client instance.
        /// </returns>
        public IFluentClient<ODataEntry> For(ODataExpression expression)
        {
            return new FluentClient<ODataEntry>(this, _session).For(expression);
        }

        /// <summary>
        /// Returns an instance of a fluent OData client for the specified collection.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns>
        /// The fluent OData client instance.
        /// </returns>
        public IFluentClient<T> For<T>(string collectionName = null)
            where T : class
        {
            return new FluentClient<T>(this, _session).For(collectionName);
        }

        private FluentClient<IDictionary<string, object>> GetFluentClient()
        {
            return new FluentClient<IDictionary<string, object>>(this, _session);
        }

        /// <summary>
        /// Sets the word pluralizer used when resolving metadata objects.
        /// </summary>
        /// <param name="pluralizer">The pluralizer.</param>
        public void SetPluralizer(IPluralizer pluralizer)
        {
            _session.Pluralizer = pluralizer;
        }
    }
}
