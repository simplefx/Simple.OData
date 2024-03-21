namespace Simple.OData.Client.V4.Adapter.Extensions;

public static class ODataClientExtensions
{
	/// <summary>
	/// Adds OData extensions e.g. data aggregation extensions to the OData client
	/// </summary>
	/// <param name="client">OData client to extend</param>
	/// <returns>Extended OData client</returns>
	public static IExtendedODataClient WithExtensions(this IODataClient client)
	{
		if (client is not ODataClient oDataClient)
		{
			throw new ArgumentException("Client should be ODataClient");
		}

		return WithExtensions(oDataClient);
	}

	/// <summary>
	/// Adds OData extensions e.g. data aggregation extensions to the OData client
	/// </summary>
	/// <param name="client">OData client to extend</param>
	/// <returns>Extended OData client</returns>
	public static ExtendedODataClient WithExtensions(this ODataClient client)
	{
		return new ExtendedODataClient(client);
	}
}
