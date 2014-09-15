using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    using Entry = System.Collections.Generic.Dictionary<string, object>;

    public class BatchODataTestsV2Atom : BatchODataTests
    {
        public BatchODataTestsV2Atom() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Atom) { }
    }

    public class BatchODataTestsV2Json : BatchODataTests
    {
        public BatchODataTestsV2Json() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public class BatchODataTestsV3Atom : BatchODataTests
    {
        public BatchODataTestsV3Atom() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Atom) { }
    }

    public class BatchODataTestsV3Json : BatchODataTests
    {
        public BatchODataTestsV3Json() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public class BatchODataTestsV4Json : BatchODataTests
    {
        public BatchODataTestsV4Json() : base(ODataV4ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public abstract class BatchODataTests : ODataTests
    {
        protected BatchODataTests(string serviceUri, ODataPayloadFormat payloadFormat) : base(serviceUri, payloadFormat) { }

        [Fact]
        public async Task Success()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", new Entry() { { "Name", "Test1" }, { "Price", 10m } }, false);
                await client.InsertEntryAsync("Products", new Entry() { { "Name", "Test2" }, { "Price", 20m } }, false);
                await batch.CompleteAsync();
            }

            var product = await _client.FindEntryAsync("Products?$filter=Name eq 'Test1'");
            Assert.NotNull(product);
            product = await _client.FindEntryAsync("Products?$filter=Name eq 'Test2'");
            Assert.NotNull(product);
        }

        [Fact]
        public async Task PartialFailures()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", new Entry() { { "Name", "Test1" }, { "Price", 10m } }, false);
                await client.InsertEntryAsync("Products", new Entry() { { "Name", "Test2" }, { "Price", 10m }, { "SupplierID", 0xFFFF } }, false);
                await AssertThrowsAsync<WebRequestException>(async () => await batch.CompleteAsync());
            }
        }

        [Fact]
        public async Task AllFailures()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", new Entry() { { "Price", 10m } }, false);
                await client.InsertEntryAsync("Products", new Entry() { { "Price", 20m } }, false);
                await AssertThrowsAsync<WebRequestException>(async () => await batch.CompleteAsync());
            }
        }

        [Fact]
        public async Task InsertUpdateDeleteSingleBatch()
        {
            var key = new Entry() { { "Name", "Test11" } };

            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", new Entry() { { "Name", "Test11" }, { "Price", 21m } }, false);
                await client.UpdateEntriesAsync("Products", "Products?$filter=Name eq 'Test11'", new Entry() { { "Price", 22m } });
                await client.DeleteEntriesAsync("Products", "Products?$filter=Name eq 'Test11'");
                await batch.CompleteAsync();
            }

            var product = await _client.FindEntryAsync("Products?$filter=Name eq 'Test11'");
            Assert.Equal(21m, product["Price"]);
        }

        [Fact]
        public async Task InsertUpdateDeleteSeparateBatches()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", new Entry() { { "Name", "Test12" }, { "Price", 21m } }, false);
                await batch.CompleteAsync();
            }
            var product = await _client.FindEntryAsync("Products?$filter=Name eq 'Test12'");
            Assert.Equal(21m, product["Price"]);
            var key = new Entry() { { "ID", product["ID"] } };

            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.UpdateEntryAsync("Products", key, new Entry() { { "Price", 22m } });
                await batch.CompleteAsync();
            }
            product = await _client.FindEntryAsync("Products?$filter=Name eq 'Test12'");
            Assert.Equal(22m, product["Price"]);

            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.DeleteEntryAsync("Products", key);
                await batch.CompleteAsync();
            }
            product = await _client.FindEntryAsync("Products?$filter=Name eq 'Test12'");
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
                await client.InsertEntryAsync("Products", new Entry() { { "Name", "Test14" }, { "Price", 21m }, { "Category", category } }, false);
                await batch.CompleteAsync();
            }

            var product = await _client
                .For("Products")
                .Expand("Category")
                .Filter("Name eq 'Test14'")
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
                var product1 = await client.InsertEntryAsync("Products", new Entry() { { "Name", "Test15" }, { "Price", 21m } });
                var product2 = await client.InsertEntryAsync("Products", new Entry() { { "Name", "Test16" }, { "Price", 22m } });
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
