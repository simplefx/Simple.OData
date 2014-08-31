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
        private readonly Schema _schema;
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
            _schema = Schema.FromUrl(_settings.UrlBase, _settings.Credentials);

            _requestBuilder = new CommandRequestBuilder(_settings.UrlBase, _settings.Credentials);
            _requestRunner = new CommandRequestRunner(_schema, _settings);
            _requestRunner.BeforeRequest = _settings.BeforeRequest;
            _requestRunner.AfterResponse = _settings.AfterResponse;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataClient"/> class.
        /// </summary>
        /// <param name="batch">The OData batch instance.</param>
        public ODataClient(ODataBatch batch)
        {
            _settings = batch.Settings;
            _schema = Schema.FromUrl(_settings.UrlBase, _settings.Credentials);

            _requestBuilder = batch.RequestBuilder;
            _requestRunner = batch.RequestRunner;
        }

        internal Schema Schema
        {
            get { return _schema; }
        }

        /// <summary>
        /// Parses the OData service metadata schema string.
        /// </summary>
        /// <typeparam name="T">OData protocol specific metadata interface</typeparam>
        /// <param name="metadataString">The metadata string.</param>
        /// <returns>
        /// The schema.
        /// </returns>
        public static T ParseMetadataString<T>(string metadataString)
        {
            return (T)Client.Schema.FromMetadata(metadataString).Provider.Model;
        }

        /// <summary>
        /// Sets the word pluralizer used when resolving metadata objects.
        /// </summary>
        /// <param name="pluralizer">The pluralizer.</param>
        public static void SetPluralizer(IPluralizer pluralizer)
        {
            StringExtensions.SetPluralizer(pluralizer);
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
            return new FluentClient<ODataEntry>(this, _schema).For(expression);
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
            return new FluentClient<T>(this, _schema).For(collectionName);
        }

        private FluentClient<IDictionary<string, object>> GetFluentClient()
        {
            return new FluentClient<IDictionary<string, object>>(this, _schema);
        }
    }
}
