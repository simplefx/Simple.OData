using System;
using System.Linq;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class WebApiTests : IDisposable
    {
        protected string _serviceUri;
        protected IODataClient _client;

        public WebApiTests()
        {
            _client = new ODataClient("http://" + "WEBAPI-PRODUCTS/odata");
        }

        public void Dispose()
        {
            if (_client != null)
            {
                var products = _client.FindEntries("Products");
                foreach (var product in products)
                {
                    if (product["Name"].ToString().StartsWith("Test"))
                        _client.DeleteEntry("Products", product);
                }
            }
        }

        [Fact]
        public void GetProductsCount()
        {
            var products = _client
                .For("Products")
                .FindEntries();

            Assert.Equal(5, products.Count());
        }

        [Fact]
        public void InsertProduct()
        {
            var product = _client
                .For("Products")
                .Set(new Entry() { { "Name", "Test1" }, { "Price", 18m } })
                .InsertEntry();

            Assert.Equal("Test1", product["Name"]);
        }

        [Fact]
        public void UpdateProduct()
        {
            var product = _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntry();

            product = _client
                .For("Products")
                .Key(product["ID"])
                .Set(new { Price = 123m })
                .UpdateEntry();

            Assert.Equal(123m, product["Price"]);
        }

        [Fact]
        public void DeleteProduct()
        {
            var product = _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntry();

            _client
                .For("Products")
                .Key(product["ID"])
                .DeleteEntry();

            product = _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .FindEntry();

            Assert.Null(product);
        }
    }
}
