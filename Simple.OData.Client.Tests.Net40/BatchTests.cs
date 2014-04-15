using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    using Entry = System.Collections.Generic.Dictionary<string, object>;

    public class BatchTests : TestBase
    {
        [Fact]
        public async Task Success()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 10m } }, false);
                await client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test2" }, { "UnitPrice", 20m } }, false);
                await batch.CompleteAsync();
            }

            var product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test1'");
            Assert.NotNull(product);
            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test2'");
            Assert.NotNull(product);
        }

        [Fact]
        public async Task PartialFailures()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 10m } }, false);
                await client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test2" }, { "UnitPrice", 10m }, { "SupplierID", 0xFFFF } }, false);
                await AssertThrowsAsync<WebRequestException>(async () => await batch.CompleteAsync());
            }
        }

        [Fact]
        public async Task AllFailures()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", new Entry() { { "UnitPrice", 10m } }, false);
                await client.InsertEntryAsync("Products", new Entry() { { "UnitPrice", 20m } }, false);
                await AssertThrowsAsync<WebRequestException>(async () => await batch.CompleteAsync());
            }
        }

        [Fact]
        public async Task InsertUpdateDeleteSingleBatch()
        {
            var key = new Entry() { { "ProductName", "Test11" } };

            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test11" }, { "UnitPrice", 21m } }, false);
                await client.UpdateEntriesAsync("Products", "Products?$filter=ProductName eq 'Test11'", new Entry() { { "UnitPrice", 22m } });
                await client.DeleteEntriesAsync("Products", "Products?$filter=ProductName eq 'Test11'");
                await batch.CompleteAsync();
            }

            var product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test11'");
            Assert.Equal(21m, product["UnitPrice"]);
        }

        [Fact]
        public async Task InsertUpdateDeleteSeparateBatches()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test12" }, { "UnitPrice", 21m } }, false);
                await batch.CompleteAsync();
            }
            var product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test12'");
            Assert.Equal(21m, product["UnitPrice"]);
            var key = new Entry() { { "ProductID", product["ProductID"] } };

            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.UpdateEntryAsync("Products", key, new Entry() { { "UnitPrice", 22m } });
                await batch.CompleteAsync();
            }
            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test12'");
            Assert.Equal(22m, product["UnitPrice"]);

            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.DeleteEntryAsync("Products", key);
                await batch.CompleteAsync();
            }
            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test12'");
            Assert.Null(product);
        }

        [Fact]
        public async Task InsertSingleEntityWithSingleAssociationSingleBatch()
        {
            IDictionary<string, object> category;
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                category = await client.InsertEntryAsync("Categories", new Entry() { { "CategoryName", "Test13" } });
                client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test14" }, { "UnitPrice", 21m }, { "Category", category } }, false);
                await batch.CompleteAsync();
            }

            var product = await _client
                .For("Products")
                .Expand("Category")
                .Filter("ProductName eq 'Test14'")
                .FindEntryAsync();
            Assert.Equal("Test13", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public async Task InsertSingleEntityWithMultipleAssociationsSingleBatch()
        {
            IDictionary<string, object> category;
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                var product1 = await client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test15" }, { "UnitPrice", 21m } });
                var product2 = await client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test16" }, { "UnitPrice", 22m } });
                await client.InsertEntryAsync("Categories", new Entry() { { "CategoryName", "Test17" }, { "Products", new[] { product1, product2 } } }, false);
                await batch.CompleteAsync();
            }

            category = await _client
                .For("Categories")
                .Expand("Products")
                .Filter("CategoryName eq 'Test17'")
                .FindEntryAsync();
            Assert.Equal(2, (category["Products"] as IEnumerable<object>).Count());
        }
    }
}
