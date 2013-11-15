using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public partial class ODataClient
    {
        private readonly ODataClientSettings _settings;
        private readonly ISchema _schema;
        private readonly RequestBuilder _requestBuilder;
        private readonly RequestRunner _requestRunner;

        public ODataClient(string urlBase)
            : this(new ODataClientSettings { UrlBase = urlBase })
        {
        }

        public ODataClient(ODataClientSettings settings)
        {
            _settings = settings;
            _schema = Client.Schema.Get(_settings.UrlBase, _settings.Credentials);

            _requestBuilder = new CommandRequestBuilder(_settings.UrlBase, _settings.Credentials);
            _requestRunner = new CommandRequestRunner(_schema, _settings);
            _requestRunner.BeforeRequest = _settings.BeforeRequest;
            _requestRunner.AfterResponse = _settings.AfterResponse;
        }

        public ODataClient(ODataBatch batch)
        {
            _settings = batch.Settings;
            _schema = Client.Schema.Get(_settings.UrlBase, _settings.Credentials);

            _requestBuilder = batch.RequestBuilder;
            _requestRunner = batch.RequestRunner;
        }

        public ISchema Schema
        {
            get { return _schema; }
        }

        public string SchemaAsString
        {
            get { return SchemaProvider.FromUrl(_settings.UrlBase, _settings.Credentials).SchemaAsString; }
        }

        public static ISchema ParseSchemaString(string schemaString)
        {
            return SchemaProvider.FromMetadata(schemaString).Schema;
        }

        public static void SetPluralizer(IPluralizer pluralizer)
        {
            StringExtensions.SetPluralizer(pluralizer);
        }

        public IFluentClient<IDictionary<string, object>> For(string collectionName)
        {
            return new FluentClient<IDictionary<string, object>>(this).For(collectionName);
        }

        public IFluentClient<ODataEntry> For(ODataExpression expression)
        {
            return new FluentClient<ODataEntry>(this).For(expression);
        }

        public IFluentClient<T> For<T>(string collectionName = null)
            where T : class
        {
            return new FluentClient<T>(this).For(collectionName);
        }

        public string FormatFilter(string collection, ODataExpression expression)
        {
            return FormatFilterExpression(collection, expression);
        }

        public string FormatFilter<T>(string collection, Expression<Func<T, bool>> expression)
        {
            return FormatFilterExpression(collection, ODataExpression.FromLinqExpression(expression.Body));
        }

        private string FormatFilterExpression(string collection, ODataExpression expression)
        {
            var clientWithCommand = new FluentClient<IDictionary<string, object>>(this);
            var filter = expression.Format(clientWithCommand.Schema, collection);

            return clientWithCommand
                .For(collection)
                .Filter(filter).CommandText;
        }
    }
}
