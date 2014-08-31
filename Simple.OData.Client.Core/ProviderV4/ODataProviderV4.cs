using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class ODataProviderV4 : ODataProvider
    {
        private readonly string _urlBase;

        public new IEdmModel Model
        {
            get { return base.Model as IEdmModel; }
            set { base.Model = value; }
        }

        public ODataProviderV4(string urlBase)
        {
            _urlBase = urlBase;
        }

        public ODataProviderV4(string urlBase, string protocolVersion, HttpResponseMessage response)
        {
            _urlBase = urlBase;
            ProtocolVersion = protocolVersion;

            using (var messageReader = new ODataMessageReader(new ODataV4ResponseMessage(response)))
            {
                Model = messageReader.ReadMetadataDocument();
            }
        }

        public ODataProviderV4(string urlBase, string metadataString, string protocolVersion)
        {
            _urlBase = urlBase;
            ProtocolVersion = protocolVersion;

            var reader = XmlReader.Create(new StringReader(metadataString));
            reader.MoveToContent();
            Model = EdmxReader.Parse(reader);
        }

        public override IMetadata GetMetadata()
        {
            return new MetadataV4(Model);
        }

        public override IResponseReader GetResponseReader()
        {
            throw new NotImplementedException();
        }

        public override IRequestWriter GetRequestWriter()
        {
            throw new NotImplementedException();
        }

        public EdmSchema CreateEdmSchema(ODataProvider provider)
        {
            return new EdmSchema(new EdmModelParserV4(provider.Model as IEdmModel));
        }
    }
}