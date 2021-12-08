namespace Simple.OData.Client.Tests
{
	public interface IAddress
	{
		AddressType Type { get; set; }

		string City { get; set; }

		string Region { get; set; }

		string PostalCode { get; set; }

		string Country { get; set; }
	}
}
