using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class LinkDynamicTests : TestBase
    {
        [Fact]
        public void LinkEntry()
        {
            var x = ODataDynamic.Expression;
            var category = _client
                .For(x.Categories)
                .Set(x.CategoryName = "Test4")
                .InsertEntry();
            var product = _client
                .For(x.Products)
                .Set(x.ProductName = "Test5")
                .InsertEntry();

            _client
                .For(x.Products)
                .Key(product)
                .LinkEntry(x.Category, category);

            product = _client
                .For(x.Products)
                .Filter(x.ProductName == "Test5")
                .FindEntry();
            Assert.NotNull(product["CategoryID"]);
            Assert.Equal(category["CategoryID"], product["CategoryID"]);
        }

        [Fact]
        public void UnlinkEntry()
        {
            var x = ODataDynamic.Expression;
            var category = _client
                .For(x.Categories)
                .Set(x.CategoryName = "Test4")
                .InsertEntry();
            var product = _client
                .For(x.Products)
                .Set(x.ProductName = "Test5", x.CategoryID = category["CategoryID"])
                .InsertEntry();

            _client
                .For(x.Products)
                .Key(product)
                .UnlinkEntry(x.Category);

            product = _client
                .For(x.Products)
                .Filter(x.ProductName == "Test5")
                .FindEntry();
            Assert.Null(product["CategoryID"]);
        }
    }
}
