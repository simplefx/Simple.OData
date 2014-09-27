using System;
using System.IO;
using System.Net.Http;
using System.Xml;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;

namespace Simple.OData.Client.V4.Adapter
{
    public class ODataAdapter : ODataAdapterBase
    {
        private readonly ISession _session;

        public override AdapterVersion AdapterVersion { get { return AdapterVersion.V4; } }

        public new IEdmModel Model
        {
            get { return base.Model as IEdmModel; }
            set { base.Model = value; }
        }

        public ODataAdapter(ISession session, string protocolVersion, HttpResponseMessage response)
        {
            _session = session;
            ProtocolVersion = protocolVersion;

            using (var messageReader = new ODataMessageReader(new ODataResponseMessage(response)))
            {
                Model = messageReader.ReadMetadataDocument();
            }
        }

        public ODataAdapter(ISession session, string protocolVersion, string metadataString)
        {
            _session = session;
            ProtocolVersion = protocolVersion;

            var reader = XmlReader.Create(new StringReader(metadataString));
            reader.MoveToContent();
            Model = EdmxReader.Parse(reader);
        }

        public override string GetODataVersionString()
        {
            switch (this.ProtocolVersion)
            {
                case ODataProtocolVersion.V4:
                    return "V4";
            }
            throw new InvalidOperationException(string.Format("Unsupported OData protocol version: \"{0}\"", this.ProtocolVersion));
        }

        public override IMetadata GetMetadata()
        {
            return new Metadata(_session, Model);
        }

        public override IResponseReader GetResponseReader()
        {
            return new ResponseReader(_session, Model);
        }

        public override IRequestWriter GetRequestWriter(Lazy<IBatchWriter> deferredBatchWriter)
        {
            return new RequestWriter(_session, Model, deferredBatchWriter);
        }

        public override IBatchWriter GetBatchWriter()
        {
            return new BatchWriter(_session);
        }
    }
}