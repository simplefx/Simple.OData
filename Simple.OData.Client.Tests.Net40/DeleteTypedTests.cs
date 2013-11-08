using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class DeleteTypedTests : TestBase
    {
        [Fact]
        public void DeleteByKey()
        {
            var product = _client
                .For<Product>()
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            _client
                .For<Product>()
                .Key(product.ProductID)
                .DeleteEntry();

            product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .FindEntry();

            Assert.Null(product);
        }

        [Fact]
        public void DeleteByFilter()
        {
            var product = _client
                .For<Product>()
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            _client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .DeleteEntry();

            product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .FindEntry();

            Assert.Null(product);
        }

        [Fact]
        public void DeleteByObjectAsKey()
        {
            var product = _client
                .For<Product>()
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            _client
                .For<Product>()
                .Key(product)
                .DeleteEntry();

            product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .FindEntry();

            Assert.Null(product);
        }
    }
}
