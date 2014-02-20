using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public partial class ODataClient : IODataClient
    {
        private readonly ODataClientSettings _settings;
        private readonly ISchema _schema;
        private readonly RequestBuilder _requestBuilder;
        private readonly RequestRunner _requestRunner;

        public ODataClient(string urlBase)
            : this(new ODataClientSettings {UrlBase = urlBase})
        {
        }

        public ODataClient(ODataClientSettings settings)
        {
            _settings = settings;
            _schema = Client.Schema.FromUrl(_settings.UrlBase, _settings.Credentials);

            _requestBuilder = new CommandRequestBuilder(_settings.UrlBase, _settings.Credentials);
            _requestRunner = new CommandRequestRunner(_schema, _settings);
            _requestRunner.BeforeRequest = _settings.BeforeRequest;
            _requestRunner.AfterResponse = _settings.AfterResponse;
        }

        public ODataClient(ODataBatch batch)
        {
            _settings = batch.Settings;
            _schema = Client.Schema.FromUrl(_settings.UrlBase, _settings.Credentials);

            _requestBuilder = batch.RequestBuilder;
            _requestRunner = batch.RequestRunner;
        }

        public ISchema Schema
        {
            get { return _schema; }
        }

        public string SchemaAsString
        {
            get { return SchemaAsStringAsync.Result; }
        }

        public Task<string> SchemaAsStringAsync
        {
            get { return GetSchemaAsStringAsync(_settings.UrlBase, _settings.Credentials); }
        }

        public static ISchema ParseSchemaString(string schemaString)
        {
            return Client.Schema.FromMetadata(schemaString);
        }

        public static void SetPluralizer(IPluralizer pluralizer)
        {
            StringExtensions.SetPluralizer(pluralizer);
        }

        public IFluentClient<IDictionary<string, object>> For(string collectionName)
        {
            return GetFluentClient().For(collectionName);
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

        public string FormatCommand(string collection, ODataExpression expression)
        {
            return GetCommandText(collection, expression);
        }

        public string FormatCommand<T>(string collection, Expression<Func<T, bool>> expression)
        {
            return GetCommandText(collection, ODataExpression.FromLinqExpression(expression.Body));
        }

        private string GetCommandText(string collection, ODataExpression expression)
        {
            return GetFluentClient()
                .For(collection)
                .Filter(expression.Format(this.Schema, collection))
                .CommandText;
        }

        private FluentClient<IDictionary<string, object>> GetFluentClient()
        {
            return new FluentClient<IDictionary<string, object>>(this);
        }
    }
}
