using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Simple.OData.Client.TestUtils;

namespace Simple.OData.Client.Tests
{
    public class LinkDynamicTests : TestBase
    {
        [Fact]
        public async Task LinkEntry()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var category = await client
                .For(x.Categories)
                .Set(x.CategoryName = "Test4")
                .InsertEntryAsync();
            var product = await client
                .For(x.Products)
                .Set(x.ProductName = "Test5")
                .InsertEntryAsync();

            await client
                .For(x.Products)
                .Key(product)
                .LinkEntryAsync(x.Category, category);

            product = await client
                .For(x.Products)
                .Filter(x.ProductName == "Test5")
                .FindEntryAsync();
            Assert.NotNull(product.CategoryID);
            Assert.Equal(category.CategoryID, product.CategoryID);
        }

        [Fact]
        public async Task UnlinkEntry()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var category = await client
                .For(x.Categories)
                .Set(x.CategoryName = "Test4")
                .InsertEntryAsync();
            var product = await client
                .For(x.Products)
                .Set(x.ProductName = "Test5", x.CategoryID = category.CategoryID)
                .InsertEntryAsync();

            await client
                .For(x.Products)
                .Key(product)
                .UnlinkEntryAsync(x.Category);

            product = await client
                .For(x.Products)
                .Filter(x.ProductName == "Test5")
                .FindEntryAsync();
            Assert.Null(product.CategoryID);
        }
    }
}
