using System.IO;
using System.Net.Http;
using System.Xml;

using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public static class V4ModelAdapter
    {
        public static void Reference() { }
    }
}

#pragma warning disable 1591

namespace Simple.OData.Client.V4.Adapter
{
    public class ODataModelAdapter : ODataModelAdapterBase
    {
        public override AdapterVersion AdapterVersion => AdapterVersion.V4;

        public new IEdmModel Model
        {
            get => base.Model as IEdmModel;
            set => base.Model = value;
        }

        private ODataModelAdapter(string protocolVersion)
        {
            ProtocolVersion = protocolVersion;
        }

        public ODataModelAdapter(string protocolVersion, HttpResponseMessage response)
            : this(protocolVersion)
        {
            var readerSettings = new ODataMessageReaderSettings
            {
                MessageQuotas = { MaxReceivedMessageSize = int.MaxValue }
            };
            using (var messageReader = new ODataMessageReader(new ODataResponseMessage(response), readerSettings))
            {
                Model = messageReader.ReadMetadataDocument();
            }
        }

        public ODataModelAdapter(string protocolVersion, string metadataString)
            : this(protocolVersion)
        {
            // HACK to prevent failure due to unsupported ConcurrencyMode attribute
            metadataString = metadataString
                .Replace(" ConcurrencyMode=\"None\"", "")
                .Replace(" ConcurrencyMode=\"Fixed\"", "");
            using (var reader = XmlReader.Create(new StringReader(metadataString)))
            {
                reader.MoveToContent();
                Model = CsdlReader.Parse(reader);
            }
        }
    }
}