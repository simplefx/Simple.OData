using System;
using System.Linq;
using Simple.OData.Client.TestUtils;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class WebApiTests : TestBase
    {
        [Fact]
        public void GetProductsCount()
        {
            var client = new ODataClient("http://localhost:51849/odata");

            var products = client
                .For("Products")
                .FindEntries();

            Assert.Equal(5, products.Count());
        }
    }
}
