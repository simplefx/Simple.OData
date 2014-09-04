using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;

namespace Simple.OData.Client
{
    class ODataProviderV4 : ODataProvider
    {
        private readonly ISession _session;
        private readonly string _urlBase;

        public new IEdmModel Model
        {
            get { return base.Model as IEdmModel; }
            set { base.Model = value; }
        }

        public ODataProviderV4(ISession session, string urlBase, string protocolVersion, HttpResponseMessage response)
        {
            _session = session;
            _urlBase = urlBase;
            ProtocolVersion = protocolVersion;

            using (var messageReader = new ODataMessageReader(new ODataV4ResponseMessage(response)))
            {
                Model = messageReader.ReadMetadataDocument();
            }
        }

        public ODataProviderV4(ISession session, string urlBase, string protocolVersion, string metadataString)
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
            return new MetadataV4(_session, Model);
        }

        public override IResponseReader GetResponseReader()
        {
            throw new NotImplementedException();
        }

        public override IRequestWriter GetRequestWriter()
        {
            throw new NotImplementedException();
        }
    }
}