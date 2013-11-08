using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class DeleteDynamicTests : TestBase
    {
        [Fact]
        public void DeleteByKey()
        {
            var x = ODataDynamic.Expression;
            var product = _client
                .For(x.Products)
                .Set(x.ProductName = "Test1", x.UnitPrice = 18m)
                .InsertEntry();

            _client
                .For(x.Products)
                .Key(x.ProductID)
                .DeleteEntry();

            product = _client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .FindEntry();

            Assert.Null(product);
        }

        [Fact]
        public void DeleteByFilter()
        {
            var x = ODataDynamic.Expression;
            var product = _client
                .For(x.Products)
                .Set(x.ProductName = "Test1", x.UnitPrice = 18m)
                .InsertEntry();

            _client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .DeleteEntry();

            product = _client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .FindEntry();

            Assert.Null(product);
        }

        [Fact]
        public void DeleteByObjectAsKey()
        {
            var x = ODataDynamic.Expression;
            var product = _client
                .For(x.Products)
                .Set(x.ProductName = "Test1", x.UnitPrice = 18m)
                .InsertEntry();

            _client
                .For(x.Products)
                .Key(product)
                .DeleteEntry();

            product = _client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .FindEntry();

            Assert.Null(product);
        }
    }
}
