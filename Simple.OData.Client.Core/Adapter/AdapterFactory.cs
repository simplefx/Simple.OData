using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Simple.OData.Client
{
    class AdapterFactory
    {
        private readonly ISession _session;

        public AdapterFactory(ISession session)
        {
            _session = session;
        }

        public async Task<ODataAdapter> CreateAdapterAsync(HttpResponseMessage response)
        {
            var protocolVersions = GetSupportedProtocolVersions(response).ToArray();

            if (protocolVersions.Any(x => x == ODataProtocolVersion.V4))
                return new ODataAdapterV4(_session, protocolVersions.First(), response);
            else if (protocolVersions.Any(x => x == ODataProtocolVersion.V1 || x == ODataProtocolVersion.V2 || x == ODataProtocolVersion.V3))
                return new ODataAdapterV3(_session, protocolVersions.First(), response);

            throw new NotSupportedException(string.Format("OData protocol {0} is not supported", protocolVersions));
        }

        public async Task<string> GetMetadataAsStringAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync();
        }

        public ODataAdapter ParseMetadata(string metadataString)
        {
            var reader = XmlReader.Create(new StringReader(metadataString));
            reader.MoveToContent();
            var protocolVersion = reader.GetAttribute("Version");

            if (protocolVersion == ODataProtocolVersion.V4)
                return new ODataAdapterV4(_session, protocolVersion, metadataString);
            else if (protocolVersion == ODataProtocolVersion.V1 || protocolVersion == ODataProtocolVersion.V2 || protocolVersion == ODataProtocolVersion.V3)
                return new ODataAdapterV3(_session, protocolVersion, metadataString);

            throw new NotSupportedException(string.Format("OData protocol {0} is not supported", protocolVersion));
        }

        internal async Task<HttpResponseMessage> SendMetadataRequestAsync(CancellationToken cancellationToken)
        {
            var request = await new RequestBuilder(_session).CreateGetRequestAsync(ODataLiteral.Metadata);
            var requestRunner = new RequestRunner(_session);

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