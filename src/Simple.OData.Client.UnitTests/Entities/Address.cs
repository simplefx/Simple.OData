namespace Simple.OData.Client.Tests
{
    public class Address : IAddress
    {
        public AddressType Type { get; set; }

        AddressType IAddress.Type
        {
            get => Type;
            set => Type = value;
        }

        public string City { get; set; }

        public string Region { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }
    }
}