using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class LinkTests : TestBase
    {
        [Fact]
        public void LinkEntry()
        {
            var category = _client
                .For("Categories")
                .Set(new { CategoryName = "Test4" })
                .InsertEntry();
            var product = _client
                .For("Products")
                .Set(new { ProductName = "Test5" })
                .InsertEntry();

            _client
                .For("Products")
                .Key(product)
                .LinkEntry("Category", category);

            product = _client
                .For("Products")
                .Filter("ProductName eq 'Test5'")
                .FindEntry();
            Assert.NotNull(product["CategoryID"]);
            Assert.Equal(category["CategoryID"], product["CategoryID"]);
        }

        [Fact]
        public void UnlinkEntry()
        {
            var category = _client
                .For("Categories")
                .Set(new { CategoryName = "Test4" })
                .InsertEntry();
            var product = _client
                .For("Products")
                .Set(new { ProductName = "Test5", CategoryID = category["CategoryID"] })
                .InsertEntry();

            _client
                .For("Products")
                .Key(product)
                .UnlinkEntry("Category");

            product = _client
                .For("Products")
                .Filter("ProductName eq 'Test5'")
                .FindEntry();
            Assert.Null(product["CategoryID"]);
        }
    }
}
