using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.OData;

namespace Simple.OData.Client
{
    class ODataProviderV3 : ODataProvider
    {
        private readonly ISession _session;
        private readonly string _urlBase;

        public new IEdmModel Model
        {
            get { return base.Model as IEdmModel; }
            set { base.Model = value; }
        }

        public ODataProviderV3(ISession session, string urlBase, string protocolVersion, HttpResponseMessage response)
        {
            _session = session;
            _urlBase = urlBase;
            ProtocolVersion = protocolVersion;

            using (var messageReader = new ODataMessageReader(new ODataV3ResponseMessage(response)))
            {
                Model = messageReader.ReadMetadataDocument();
            }
        }

        public ODataProviderV3(ISession session, string urlBase, string protocolVersion, string metadataString)
        {
            _session = session;
            _urlBase = urlBase;
            ProtocolVersion = protocolVersion;

            var reader = XmlReader.Create(new StringReader(metadataString));
            reader.MoveToContent();
            Model = EdmxReader.Parse(reader);
        }

        public override IMetadata GetMetadata()
        {
            return new MetadataV3(_session, Model);
        }

        public override IResponseReader GetResponseReader()
        {
            return new ResponseReaderV3(_session, Model);
        }

        public override IRequestWriter GetRequestWriter()
        {
            return new RequestWriterV3(_session, _urlBase, Model);
        }
    }
}