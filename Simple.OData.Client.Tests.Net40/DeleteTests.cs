using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class DeleteTests : TestBase
    {
        [Fact]
        public void DeleteByKey()
        {
            var product = _client
                .For("Products")
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            _client
                .For("Products")
                .Key(product["ProductID"])
                .DeleteEntry();

            product = _client
                .For("Products")
                .Filter("ProductName eq 'Test1'")
                .FindEntry();

            Assert.Null(product);
        }

        [Fact]
        public void DeleteByFilter()
        {
            var product = _client
                .For("Products")
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            _client
                .For("Products")
                .Filter("ProductName eq 'Test1'")
                .DeleteEntry();

            product = _client
                .For("Products")
                .Filter("ProductName eq 'Test1'")
                .FindEntry();

            Assert.Null(product);
        }

        [Fact]
        public void DeleteByObjectAsKey()
        {
            var product = _client
                .For("Products")
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            _client
                .For("Products")
                .Key(product)
                .DeleteEntry();

            product = _client
                .For("Products")
                .Filter("ProductName eq 'Test1'")
                .FindEntry();

            Assert.Null(product);
        }
    }
}
