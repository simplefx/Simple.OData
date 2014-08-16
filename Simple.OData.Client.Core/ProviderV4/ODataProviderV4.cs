using System;
using System.Net.Http;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace Simple.OData.Client
{
    class ProviderMetadataV4 : ProviderMetadata
    {
        public new IEdmModel Model
        {
            get { return base.Model as IEdmModel; }
            set { base.Model = value; }
        }
    }

    class ODataProviderV4
    {
        public EdmSchema CreateEdmSchema(ProviderMetadata providerMetadata)
        {
            return new EdmSchema(new EdmModelParserV4(providerMetadata.Model as IEdmModel));
        }

        public ProviderMetadataV4 GetMetadata(HttpResponseMessage response, string protocolVersion)
        {
            using (var messageReader = new ODataMessageReader(new ODataV4ResponseMessage(response)))
            {
                var model = messageReader.ReadMetadataDocument();
                return new ProviderMetadataV4
                {
                    ProtocolVersion = protocolVersion,
                    Model = model,
                };
            }
        }
    }
}