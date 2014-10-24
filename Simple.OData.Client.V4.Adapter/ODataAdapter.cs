using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.Spatial;

namespace Simple.OData.Client.V4.Adapter
{
    public class ODataAdapter : ODataAdapterBase
    {
        private readonly ISession _session;

        public override AdapterVersion AdapterVersion { get { return AdapterVersion.V4; } }

        public override ODataPayloadFormat DefaultPayloadFormat
        {
            get { return ODataPayloadFormat.Json; }
        }

        public new IEdmModel Model
        {
            get { return base.Model as IEdmModel; }
            set { base.Model = value; }
        }

		public ODataAdapter()
		{
			CustomConverters.RegisterTypeConverter(typeof(GeographyPoint), TypeConverters.CreateGeographyPoint);
			CustomConverters.RegisterTypeConverter(typeof(GeometryPoint), TypeConverters.CreateGeometryPoint);
		}

        private ODataAdapter(ISession session, string protocolVersion)
            : this()
        {
            _session = session;
            ProtocolVersion = protocolVersion;
        }

        public ODataAdapter(ISession session, string protocolVersion, HttpResponseMessage response)
            : this(session, protocolVersion)
        {
            var readerSettings = new ODataMessageReaderSettings
            {
                MessageQuotas = { MaxReceivedMessageSize = Int32.MaxValue }
            };
            using (var messageReader = new ODataMessageReader(new ODataResponseMessage(response), readerSettings))
            {
                Model = messageReader.ReadMetadataDocument();
            }
        }

        public ODataAdapter(ISession session, string protocolVersion, string metadataString)
            : this(session, protocolVersion)
        {
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

        public override string ConvertValueToUriLiteral(object value)
        {
            return value is ODataExpression
                ? (value as ODataExpression).AsString(_session)
                : ODataUriUtils.ConvertToUriLiteral(value,
                    (ODataVersion)Enum.Parse(typeof(ODataVersion), this.GetODataVersionString(), false), this.Model);
        }

        public override FunctionFormat FunctionFormat
        {
            get { return FunctionFormat.Key; }
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