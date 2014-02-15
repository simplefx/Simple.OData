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
        public void SyncWithSuccess()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                client.InsertEntry("Products", new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 10m } }, false);
                client.InsertEntry("Products", new Entry() { { "ProductName", "Test2" }, { "UnitPrice", 20m } }, false);
                batch.Complete();
            }

            var product = _client.FindEntry("Products?$filter=ProductName eq 'Test1'");
            Assert.NotNull(product);
            product = _client.FindEntry("Products?$filter=ProductName eq 'Test2'");
            Assert.NotNull(product);
        }

        [Fact]
        public async Task AsyncWithSuccess()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 10m } }, false);
                await client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test2" }, { "UnitPrice", 20m } }, false);
                batch.Complete();
            }

            var product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test1'");
            Assert.NotNull(product);
            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test2'");
            Assert.NotNull(product);
        }

        [Fact]
        public void SyncWithPartialFailures()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                client.InsertEntry("Products", new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 10m } }, false);
                client.InsertEntry("Products", new Entry() { { "ProductName", "Test2" }, { "UnitPrice", 10m }, { "SupplierID", 0xFFFF } }, false);
                Assert.Throws<WebRequestException>(() => batch.Complete());
            }
        }

        [Fact]
        public async Task AsyncWithPartialFailures()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 10m } }, false);
                await client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test2" }, { "UnitPrice", 10m }, { "SupplierID", 0xFFFF } }, false);
                Assert.Throws<WebRequestException>(() => batch.Complete());
            }
        }

        [Fact]
        public void SyncWithAllFailures()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                client.InsertEntry("Products", new Entry() { { "UnitPrice", 10m } }, false);
                client.InsertEntry("Products", new Entry() { { "UnitPrice", 20m } }, false);
                Assert.Throws<WebRequestException>(() => batch.Complete());
            }
        }

        [Fact]
        public async Task AsyncWithAllFailures()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", new Entry() { { "UnitPrice", 10m } }, false);
                await client.InsertEntryAsync("Products", new Entry() { { "UnitPrice", 20m } }, false);
                Assert.Throws<WebRequestException>(() => batch.Complete());
            }
        }

        [Fact]
        public void InsertUpdateDeleteSingleBatch()
        {
            var key = new Entry() { { "ProductName", "Test11" } };

            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                client.InsertEntry("Products", new Entry() { { "ProductName", "Test11" }, { "UnitPrice", 21m } }, false);
                client.UpdateEntries("Products", "Products?$filter=ProductName eq 'Test11'", new Entry() { { "UnitPrice", 22m } });
                client.DeleteEntries("Products", "Products?$filter=ProductName eq 'Test11'");
                batch.Complete();
            }

            var product = _client.FindEntry("Products?$filter=ProductName eq 'Test11'");
            Assert.Equal(21m, product["UnitPrice"]);
        }

        [Fact]
        public void InsertUpdateDeleteSeparateBatches()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                client.InsertEntry("Products", new Entry() { { "ProductName", "Test12" }, { "UnitPrice", 21m } }, false);
                batch.Complete();
            }
            var product = _client.FindEntry("Products?$filter=ProductName eq 'Test12'");
            Assert.Equal(21m, product["UnitPrice"]);
            var key = new Entry() { { "ProductID", product["ProductID"] } };

            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                client.UpdateEntry("Products", key, new Entry() { { "UnitPrice", 22m } });
                batch.Complete();
            }
            product = _client.FindEntry("Products?$filter=ProductName eq 'Test12'");
            Assert.Equal(22m, product["UnitPrice"]);

            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                client.DeleteEntry("Products", key);
                batch.Complete();
            }
            product = _client.FindEntry("Products?$filter=ProductName eq 'Test12'");
            Assert.Null(product);
        }

        [Fact]
        public void InsertSingleEntityWithSingleAssociationSingleBatch()
        {
            IDictionary<string, object> category;
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                category = client.InsertEntry("Categories", new Entry() { { "CategoryName", "Test13" } });
                client.InsertEntry("Products", new Entry() { { "ProductName", "Test14" }, { "UnitPrice", 21m }, { "Category", category } }, false);
                batch.Complete();
            }

            var product = _client
                .For("Products")
                .Expand("Category")
                .Filter("ProductName eq 'Test14'")
                .FindEntry();
            Assert.Equal("Test13", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public void InsertSingleEntityWithMultipleAssociationsSingleBatch()
        {
            IDictionary<string, object> category;
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                var product1 = client.InsertEntry("Products", new Entry() { { "ProductName", "Test15" }, { "UnitPrice", 21m } });
                var product2 = client.InsertEntry("Products", new Entry() { { "ProductName", "Test16" }, { "UnitPrice", 22m } });
                client.InsertEntry("Categories", new Entry() { { "CategoryName", "Test17" }, { "Products", new[] { product1, product2 } } }, false);
                batch.Complete();
            }

            category = _client
                .For("Categories")
                .Expand("Products")
                .Filter("CategoryName eq 'Test17'")
                .FindEntry();
            Assert.Equal(2, (category["Products"] as IEnumerable<object>).Count());
        }
    }
}
