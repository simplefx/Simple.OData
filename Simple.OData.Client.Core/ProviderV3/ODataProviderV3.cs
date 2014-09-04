using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.OData;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class ODataProviderV3 : ODataProvider
    {
        private readonly string _urlBase;

        public new IEdmModel Model
        {
            get { return base.Model as IEdmModel; }
            set { base.Model = value; }
        }

        public ODataProviderV3(string urlBase)
        {
            _urlBase = urlBase;
        }

        public ODataProviderV3(string urlBase, string protocolVersion, HttpResponseMessage response)
        {
            _urlBase = urlBase;
            ProtocolVersion = protocolVersion;

            using (var messageReader = new ODataMessageReader(new ODataV3ResponseMessage(response)))
            {
                Model = messageReader.ReadMetadataDocument();
            }
        }

        public ODataProviderV3(string urlBase, string protocolVersion, string metadataString)
        {
            _urlBase = urlBase;
            ProtocolVersion = protocolVersion;

            var reader = XmlReader.Create(new StringReader(metadataString));
            reader.MoveToContent();
            Model = EdmxReader.Parse(reader);
        }

        public override IMetadata GetMetadata()
        {
            return new MetadataV3(Model);
        }

        public override IResponseReader GetResponseReader()
        {
            return new ResponseReaderV3(Model);
        }

        public override IRequestWriter GetRequestWriter()
        {
            return new RequestWriterV3(_urlBase, Model);
        }

        //public EdmSchema CreateEdmSchema(ODataProvider providerMetadata)
        //{
        //    return new EdmSchema(new EdmModelParserV3(providerMetadata.Model as IEdmModel));
        //}
    }
}