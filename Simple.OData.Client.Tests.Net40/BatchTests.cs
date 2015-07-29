using System;
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
            var batch = new ODataBatch(_serviceUri);
            batch += c => c.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 10m } }, false);
            batch += c => c.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test2" }, { "UnitPrice", 20m } }, false);
            await batch.ExecuteAsync();

            var product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test1'");
            Assert.NotNull(product);
            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test2'");
            Assert.NotNull(product);
        }

        [Fact]
        public async Task EmptyBatch()
        {
            var batch = new ODataBatch(_serviceUri);
            await batch.ExecuteAsync();
        }

        [Fact]
        public async Task ReadOnlyBatch()
        {
            IDictionary<string, object> product = null;
            var batch = new ODataBatch(_serviceUri);
            batch += async c => product = await c.FindEntryAsync("Products");
            await batch.ExecuteAsync();

            Assert.NotNull(product);
        }

        [Fact]
        public async Task NestedBatch()
        {
            var batch1 = new ODataBatch(_serviceUri);
            batch1 += c => c.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 10m } }, false);
            batch1 += c => c.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test2" }, { "UnitPrice", 20m } }, false);

            var batch2 = new ODataBatch(_serviceUri);
            batch2 += c => c.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test3" }, { "UnitPrice", 30m } }, false);
            batch2 += c => c.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test4" }, { "UnitPrice", 40m } }, false);
            await batch2.ExecuteAsync();
            
            await batch1.ExecuteAsync();

            var product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test1'");
            Assert.NotNull(product);
            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test2'");
            Assert.NotNull(product);
            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test3'");
            Assert.NotNull(product);
            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test4'");
            Assert.NotNull(product);
        }

        [Fact]
        public async Task SuccessWithResults()
        {
            IDictionary<string, object> product1 = null;
            IDictionary<string, object> product2 = null;

            var batch = new ODataBatch(_serviceUri);
            batch += async x => { product1 = await x.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 10m }}); };
            batch += async x => { product2 = await x.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test2" }, { "UnitPrice", 20m }}); }; 
            await batch.ExecuteAsync();

            Assert.NotNull(product1["ProductID"]);
            Assert.NotNull(product2["ProductID"]);

            product1 = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test1'");
            Assert.NotNull(product1);
            product2 = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test2'");
            Assert.NotNull(product2);
        }

        [Fact]
        public async Task PartialFailures()
        {
            var batch = new ODataBatch(_serviceUri);
            batch += c => c.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 10m } }, false);
            batch += c => c.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test2" }, { "UnitPrice", 10m }, { "SupplierID", 0xFFFF } }, false);
            await AssertThrowsAsync<WebRequestException>(async () => await batch.ExecuteAsync());
        }

        [Fact]
        public async Task AllFailures()
        {
            var batch = new ODataBatch(_serviceUri);
            batch += c => c.InsertEntryAsync("Products", new Entry() { { "UnitPrice", 10m } }, false);
            batch += c => c.InsertEntryAsync("Products", new Entry() { { "UnitPrice", 20m } }, false);
            await AssertThrowsAsync<WebRequestException>(async () => await batch.ExecuteAsync());
        }

        [Fact]
        public async Task MultipleUpdatesSingleBatch()
        {
            IDictionary<string, object> product = null;
            IDictionary<string, object> product1 = null;
            IDictionary<string, object> product2 = null;

            var batch = new ODataBatch(_serviceUri);
            batch += async c => product = await c.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test11" }, { "UnitPrice", 21m } }, false);
            await batch.ExecuteAsync();

            batch = new ODataBatch(_serviceUri);
            batch += c => c.UpdateEntryAsync("Products", product, new Entry() { { "UnitPrice", 22m } });
            batch += async x => product1 = await x.FindEntryAsync("Products?$filter=ProductName eq 'Test11'");
            batch += c => c.UpdateEntryAsync("Products", product, new Entry() { { "UnitPrice", 23m } });
            batch += async x => product2 = await x.FindEntryAsync("Products?$filter=ProductName eq 'Test11'");
            await batch.ExecuteAsync();

            Assert.Equal(22m, product1["UnitPrice"]);
            Assert.Equal(23m, product2["UnitPrice"]);

            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test11'");
            Assert.Equal(23m, product["UnitPrice"]);
        }

        [Fact]
        public async Task UpdateDeleteSingleBatch()
        {
            IDictionary<string, object> product = null;
            IDictionary<string, object> product1 = null;
            IDictionary<string, object> product2 = null;

            var batch = new ODataBatch(_serviceUri);
            batch += async x => product = await x.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test11" }, { "UnitPrice", 21m } }, false);
            await batch.ExecuteAsync();

            batch = new ODataBatch(_serviceUri);
            batch += c => c.UpdateEntryAsync("Products", product, new Entry() { { "UnitPrice", 22m } });
            batch += async c => product1 = await c.FindEntryAsync("Products?$filter=ProductName eq 'Test11'");
            batch += c => c.DeleteEntryAsync("Products", product);
            batch += async c => product2 = await c.FindEntryAsync("Products?$filter=ProductName eq 'Test11'");
            await batch.ExecuteAsync();

            Assert.Equal(22m, product1["UnitPrice"]);
            Assert.Null(product2);

            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test11'");
            Assert.Null(product);
        }

        [Fact]
        public async Task InsertUpdateDeleteSeparateBatches()
        {
            var batch = new ODataBatch(_serviceUri);
            batch += c => c.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test12" }, { "UnitPrice", 21m } }, false);
            await batch.ExecuteAsync();

            var product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test12'");
            Assert.Equal(21m, product["UnitPrice"]);
            var key = new Entry() { { "ProductID", product["ProductID"] } };

            batch = new ODataBatch(_serviceUri);
            batch += c => c.UpdateEntryAsync("Products", key, new Entry() { { "UnitPrice", 22m } });
            await batch.ExecuteAsync();
            
            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test12'");
            Assert.Equal(22m, product["UnitPrice"]);

            batch = new ODataBatch(_serviceUri);
            batch += c => c.DeleteEntryAsync("Products", key);
            await batch.ExecuteAsync();

            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test12'");
            Assert.Null(product);
        }

        [Fact]
        public async Task InsertUpdateDeleteSeparateBatchesReuseHttpConnection()
        {
            var client = new ODataClient(new ODataClientSettings {BaseUri = _serviceUri, ReuseHttpConnection = true});
            var batch = new ODataBatch(client);
            batch += c => c.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test12" }, { "UnitPrice", 21m } }, false);
            await batch.ExecuteAsync();

            var product = await client.FindEntryAsync("Products?$filter=ProductName eq 'Test12'");
            Assert.Equal(21m, product["UnitPrice"]);
            var key = new Entry() { { "ProductID", product["ProductID"] } };

            batch = new ODataBatch(client);
            batch += c => c.UpdateEntryAsync("Products", key, new Entry() { { "UnitPrice", 22m } });
            await batch.ExecuteAsync();

            product = await client.FindEntryAsync("Products?$filter=ProductName eq 'Test12'");
            Assert.Equal(22m, product["UnitPrice"]);

            batch = new ODataBatch(client);
            batch += c => c.DeleteEntryAsync("Products", key);
            await batch.ExecuteAsync();

            product = await client.FindEntryAsync("Products?$filter=ProductName eq 'Test12'");
            Assert.Null(product);
        }

        [Fact]
        public async Task InsertSingleEntityWithSingleAssociationSingleBatch()
        {
            IDictionary<string, object> category = null;
            var batch = new ODataBatch(_serviceUri);
            batch += async x => category = await x.InsertEntryAsync("Categories", new Entry() { { "CategoryName", "Test13" } });
            batch += c => c.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test14" }, { "UnitPrice", 21m }, { "Category", category } }, false);
            await batch.ExecuteAsync();

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
            IDictionary<string, object> product1 = null;
            IDictionary<string, object> product2 = null;
            var batch = new ODataBatch(_serviceUri);
            batch += async c => product1 = await c.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test15" }, { "UnitPrice", 21m } });
            batch += async c => product2 = await c.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test16" }, { "UnitPrice", 22m } });
            batch += c => c.InsertEntryAsync("Categories", new Entry() { { "CategoryName", "Test17" }, { "Products", new[] { product1, product2 } } }, false);
            await batch.ExecuteAsync();

            var category = await _client
                .For("Categories")
                .Expand("Products")
                .Filter("CategoryName eq 'Test17'")
                .FindEntryAsync();
            Assert.Equal(2, (category["Products"] as IEnumerable<object>).Count());
        }

        [Fact]
        public async Task FunctionBatch()
        {
            var batch = new ODataBatch(_serviceUri);
            int result = 0;
            batch += async c => result = await c.Unbound().Function("ParseInt").Set(new Entry() { { "number", "1" } }).ExecuteAsScalarAsync<int>();
            await batch.ExecuteAsync();

            Assert.Equal(1, result);
        }
    }
}
