using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task<EdmSchema> GetSchemaAsync(HttpResponseMessage response)
        {
            var protocolVersions = GetSupportedProtocolVersions(response).ToArray();

            if (protocolVersions.Any(x => x == "4.0"))
                return new ODataProviderV4().CreateEdmSchema(response);
            else if (protocolVersions.Any(x => x == "1.0" || x == "2.0" || x == "3.0"))
                return new ODataProviderV3().CreateEdmSchema(response);

            throw new NotImplementedException();
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