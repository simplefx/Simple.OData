using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    using Entry = System.Collections.Generic.Dictionary<string, object>;

    public class AsyncTests : TestBase
    {
        [Fact]
        public async Task FindEntries()
        {
            var products = await _client.FindEntriesAsync("Products");
            Assert.True(products.Count() > 0);
        }

        [Fact]
        public async Task FindEntriesNonExisting()
        {
            var products = await _client.FindEntriesAsync("Products?$filter=ProductID eq -1");
            Assert.True(products.Count() == 0);
        }

        [Fact]
        public async Task FindEntriesNonExistingLong()
        {
            var products = await _client.FindEntriesAsync("Products?$filter=ProductID eq 999999999999L");
            Assert.True(products.Count() == 0);
        }

        [Fact]
        public async Task FindEntriesWithSelect()
        {
            var products = await _client.For("Products").Select("ProductName").FindEntriesAsync();
            Assert.Equal(1, products.First().Count);
            Assert.Equal("ProductName", products.First().First().Key);
        }

        [Fact]
        public async Task FindEntriesWithSelectHomogenize()
        {
            var products = await _client.For("Products").Select("Product_Name").FindEntriesAsync();
            Assert.Equal(1, products.First().Count);
            Assert.Equal("ProductName", products.First().First().Key);
        }

        [Fact]
        public async Task FindEntryExisting()
        {
            var product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Chai'");
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public async Task FindEntryNonExisting()
        {
            var product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'XYZ'");
            Assert.Null(product);
        }

        [Fact]
        public async Task FindEntryWithSelect()
        {
            var product = await _client.For("Products").Select("ProductName").FindEntryAsync();
            Assert.Equal("ProductName", product.First().Key);
        }

        [Fact]
        public async Task FindEntryWithSelectHomogenize()
        {
            var product = await _client.For("Products").Select("Product_Name").FindEntryAsync();
            Assert.Equal("ProductName", product.First().Key);
        }

        [Fact]
        public async Task GetEntryExisting()
        {
            var product = await _client.GetEntryAsync("Products", new Entry() { { "ProductID", 1 } });
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public async Task GetEntryExistingCompoundKey()
        {
            var orderDetail = await _client.GetEntryAsync("OrderDetails", new Entry() { { "OrderID", 10248 }, { "ProductID", 11 } });
            Assert.Equal(11, orderDetail["ProductID"]);
        }

        [Fact]
        public async Task GetEntryNonExisting()
        {
            await AssertThrowsAsync<WebRequestException>(async () => await _client.GetEntryAsync("Products", new Entry() { { "ProductID", -1 } }));
        }

        [Fact]
        public async Task GetEntryNonExistingIgnoreException()
        {
            var settings = new ODataClientSettings
            {
                UrlBase = _serviceUri,
                IgnoreResourceNotFoundException = true,
            };
            var client = new ODataClient(settings);
            var product = await client.GetEntryAsync("Products", new Entry() { { "ProductID", -1 } });

            Assert.Null(product);
        }

        [Fact]
        public async Task InsertEntryWithResult()
        {
            var product = await _client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 18m } }, true);

            Assert.Equal("Test1", product["ProductName"]);
        }

        [Fact]
        public async Task InsertEntryNoResult()
        {
            var product = await _client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test2" }, { "UnitPrice", 18m } }, false);

            Assert.Null(product);
        }

        [Fact]
        public async Task UpdateEntry()
        {
            var key = new Entry() { { "ProductID", 1 } };
            await _client.UpdateEntryAsync("Products", key, new Entry() { { "ProductName", "Chai" }, { "UnitPrice", 123m } });

            var product = await _client.GetEntryAsync("Products", key);
            Assert.Equal(123m, product["UnitPrice"]);
        }

        [Fact]
        public async Task DeleteEntry()
        {
            var product = await _client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test3" }, { "UnitPrice", 18m } }, true);
            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test3'");
            Assert.NotNull(product);

            await _client.DeleteEntryAsync("Products", product);

            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test3'");
            Assert.Null(product);
        }

        [Fact]
        public async Task LinkEntry()
        {
            var category = await _client.InsertEntryAsync("Categories", new Entry() { { "CategoryName", "Test4" } }, true);
            var product = await _client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test5" } }, true);

            await _client.LinkEntryAsync("Products", product, "Category", category);

            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test5'");
            Assert.NotNull(product["CategoryID"]);
            Assert.Equal(category["CategoryID"], product["CategoryID"]);
        }

        [Fact]
        public async Task UnlinkEntry()
        {
            var category = await _client.InsertEntryAsync("Categories", new Entry() { { "CategoryName", "Test6" } }, true);
            var product = await _client.InsertEntryAsync("Products", new Entry() { { "ProductName", "Test7" }, { "CategoryID", category["CategoryID"] } }, true);
            Assert.NotNull(product["CategoryID"]);
            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test7'");
            Assert.NotNull(product["CategoryID"]);
            Assert.Equal(category["CategoryID"], product["CategoryID"]);

            await _client.UnlinkEntryAsync("Products", product, "Category");

            product = await _client.FindEntryAsync("Products?$filter=ProductName eq 'Test7'");
            Assert.Null(product["CategoryID"]);
        }

        [Fact]
        public async Task ExecuteScalarFunction()
        {
            var result = await _client.ExecuteFunctionAsScalarAsync<int>("ParseInt", new Entry() { { "number", "1" } });
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task BatchWithSuccess()
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
        public async Task BatchWithPartialFailures()
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
        public async Task BatchWithAllFailures()
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
        public async Task InterceptRequest()
        {
            var settings = new ODataClientSettings
            {
                UrlBase = _serviceUri,
                BeforeRequest = x => x.Method = new HttpMethod("PUT"),
            };
            var client = new ODataClient(settings);
            await AssertThrowsAsync<WebRequestException>(async () => await client.FindEntriesAsync("Products"));
        }

        [Fact]
        public async Task InterceptResponse()
        {
            var settings = new ODataClientSettings
            {
                UrlBase = _serviceUri,
                AfterResponse = x => { throw new InvalidOperationException(); },
            };
            var client = new ODataClient(settings);
            await AssertThrowsAsync<InvalidOperationException>(async () => await client.FindEntriesAsync("Products"));
        }

        [Fact]
        public async Task FindEntryExistingDynamicFilter()
        {
            var x = ODataDynamic.Expression;
            string filter = _client.FormatCommand("Products", x.ProductName == "Chai");
            var product = await _client.FindEntryAsync(filter);
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public async Task FindEntryExistingTypedFilter()
        {
            string filter = _client.FormatCommand<Product>("Products", x => x.ProductName == "Chai");
            var product = await _client.FindEntryAsync(filter);
            Assert.Equal("Chai", product["ProductName"]);
        }

        public async static Task<T> AssertThrowsAsync<T>(Func<Task> testCode) where T : Exception
        {
            try
            {
                await testCode();
                Assert.Throws<T>(() => { });
            }
            catch (T exception)
            {
                return exception;
            }
            return null;
        }
    }
}
