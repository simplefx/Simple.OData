using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Simple.OData.Client
{
    class SchemaProvider
    {
        private readonly string _urlBase;
        private readonly ICredentials _credentials;

        public SchemaProvider(string urlBase, ICredentials credentials)
        {
            _urlBase = urlBase;
            _credentials = credentials;
        }

        public Task<string> GetSchemaAsStringAsync()
        {
            return GetSchemaAsStringAsync(CancellationToken.None);
        }

        public async Task<string> GetSchemaAsStringAsync(CancellationToken cancellationToken)
        {
            using (var response = await SendSchemaRequestAsync(cancellationToken))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> GetSchemaAsStringAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<EdmSchema> GetSchemaAsync(ProviderMetadata providerMetadata)
        {
            if (providerMetadata is ProviderMetadataV3)
                return new ODataProviderV3().CreateEdmSchema(providerMetadata);
            if (providerMetadata is ProviderMetadataV4)
                return new ODataProviderV4().CreateEdmSchema(providerMetadata);

            throw new ArgumentException(string.Format("Provider medata of type {0} is not supported", providerMetadata.GetType()), "providerMetadata");
        }

        public async Task<ProviderMetadata> GetMetadataAsync(HttpResponseMessage response)
        {
            var protocolVersions = GetSupportedProtocolVersions(response).ToArray();

            if (protocolVersions.Any(x => x == "4.0"))
                return new ODataProviderV4().GetMetadata(response, protocolVersions.First());
            else if (protocolVersions.Any(x => x == "1.0" || x == "2.0" || x == "3.0"))
                return new ODataProviderV3().GetMetadata(response, protocolVersions.First());

            throw new NotSupportedException(string.Format("OData protocol {0} is not supported", protocolVersions));
        }

        public ProviderMetadata ParseMetadata(string metadataString)
        {
            var reader = XmlReader.Create(new StringReader(metadataString));
            reader.MoveToContent();
            var protocolVersion = reader.GetAttribute("Version");

            if (protocolVersion == "4.0")
                return new ODataProviderV4().GetMetadata(metadataString, protocolVersion);
            else if (protocolVersion == "1.0" || protocolVersion == "2.0" || protocolVersion == "3.0")
                return new ODataProviderV4().GetMetadata(metadataString, protocolVersion);

            throw new NotSupportedException(string.Format("OData protocol {0} is not supported", protocolVersion));
        }

        internal async Task<HttpResponseMessage> SendSchemaRequestAsync(CancellationToken cancellationToken)
        {
            var requestBuilder = new CommandRequestBuilder(_urlBase, _credentials);
            var command = HttpCommand.Get(FluentCommand.MetadataLiteral);
            var request = requestBuilder.CreateRequest(command);
            var requestRunner = new SchemaRequestRunner(new ODataClientSettings());

            return await requestRunner.ExecuteRequestAsync(request, cancellationToken);
        }

        private IEnumerable<string> GetSupportedProtocolVersions(HttpResponseMessage response)
        {
            IEnumerable<string> headerValues;
            if (response.Headers.TryGetValues("DataServiceVersion", out headerValues) ||
                response.Headers.TryGetValues("OData-Version", out headerValues))
                return headerValues.SelectMany(x => x.Split(';')).Where(x => x.Length > 0);

            throw new InvalidOperationException("Unable to identify OData protocol version");
        }
    }
}