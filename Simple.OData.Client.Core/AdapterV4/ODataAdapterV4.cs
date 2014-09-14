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
    class ODataAdapterV4 : ODataAdapter
    {
        private readonly ISession _session;

        public new IEdmModel Model
        {
            get { return base.Model as IEdmModel; }
            set { base.Model = value; }
        }

        public ODataAdapterV4(ISession session, string protocolVersion, HttpResponseMessage response)
        {
            _session = session;
            ProtocolVersion = protocolVersion;

            using (var messageReader = new ODataMessageReader(new ODataV4ResponseMessage(response)))
            {
                Model = messageReader.ReadMetadataDocument();
            }
        }

        public ODataAdapterV4(ISession session, string protocolVersion, string metadataString)
        {
            _session = session;
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

        public override IRequestWriter GetRequestWriter(Lazy<IBatchWriter> deferredBatchWriter)
        {
            throw new NotImplementedException();
        }

        public override IBatchWriter GetBatchWriter()
        {
            throw new NotImplementedException();
        }
    }
}