using System.Xml;

using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.OData;

namespace Simple.OData.Client.V3.Adapter;

public class ODataModelAdapter : ODataModelAdapterBase
{
	public override AdapterVersion AdapterVersion => AdapterVersion.V3;

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
		using var messageReader = new ODataMessageReader(new ODataResponseMessage(response), readerSettings);
		Model = messageReader.ReadMetadataDocument();
	}

	public ODataModelAdapter(string protocolVersion, string metadataString)
		: this(protocolVersion)
	{
		using var reader = XmlReader.Create(new StringReader(metadataString));
		reader.MoveToContent();
		Model = EdmxReader.Parse(reader);
	}
}
