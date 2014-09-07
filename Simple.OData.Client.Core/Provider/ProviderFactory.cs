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
    class ProviderFactory
    {
        private readonly ISession _session;

        public ProviderFactory(ISession session)
        {
            _session = session;
        }

        public async Task<ODataProvider> CreateProviderAsync(HttpResponseMessage response)
        {
            var protocolVersions = GetSupportedProtocolVersions(response).ToArray();

            if (protocolVersions.Any(x => x == "4.0"))
                return new ODataProviderV4(_session, protocolVersions.First(), response);
            else if (protocolVersions.Any(x => x == "1.0" || x == "2.0" || x == "3.0"))
                return new ODataProviderV3(_session, protocolVersions.First(), response);

            throw new NotSupportedException(string.Format("OData protocol {0} is not supported", protocolVersions));
        }

        public Task<string> GetMetadataAsStringAsync()
        {
            return GetMetadataAsStringAsync(CancellationToken.None);
        }

        public async Task<string> GetMetadataAsStringAsync(CancellationToken cancellationToken)
        {
            using (var response = await SendMetadataRequestAsync(cancellationToken))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> GetMetadataAsStringAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync();
        }

        public ODataProvider ParseMetadata(string metadataString)
        {
            var reader = XmlReader.Create(new StringReader(metadataString));
            reader.MoveToContent();
            var protocolVersion = reader.GetAttribute("Version");

            if (protocolVersion == "4.0")
                return new ODataProviderV4(_session, protocolVersion, metadataString);
            else if (protocolVersion == "1.0" || protocolVersion == "2.0" || protocolVersion == "3.0")
                return new ODataProviderV3(_session, protocolVersion, metadataString);

            throw new NotSupportedException(string.Format("OData protocol {0} is not supported", protocolVersion));
        }

        internal async Task<HttpResponseMessage> SendMetadataRequestAsync(CancellationToken cancellationToken)
        {
            var requestBuilder = new CommandRequestBuilder(_session);
            var command = HttpCommand.Get(FluentCommand.MetadataLiteral);
            var request = requestBuilder.CreateRequest(command);
            var requestRunner = new MetadataRequestRunner(new ODataClientSettings());

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