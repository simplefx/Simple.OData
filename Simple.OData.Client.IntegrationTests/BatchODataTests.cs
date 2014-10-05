using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    using Entry = System.Collections.Generic.Dictionary<string, object>;

#if ODATA_V3
    public class BatchODataTestsV2Atom : BatchODataTests
    {
        public BatchODataTestsV2Atom() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Atom, 2) { }
    }

    public class BatchODataTestsV2Json : BatchODataTests
    {
        public BatchODataTestsV2Json() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Json, 2) { }
    }

    public class BatchODataTestsV3Atom : BatchODataTests
    {
        public BatchODataTestsV3Atom() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Atom, 3) { }
    }

    public class BatchODataTestsV3Json : BatchODataTests
    {
        public BatchODataTestsV3Json() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Json, 3) { }
    }
#endif

#if ODATA_V4
    public class BatchODataTestsV4Json : BatchODataTests
    {
        public BatchODataTestsV4Json() : base(ODataV4ReadWriteUri, ODataPayloadFormat.Json, 4) { }
    }
#endif

    public abstract class BatchODataTests : ODataTestBase
    {
        protected BatchODataTests(string serviceUri, ODataPayloadFormat payloadFormat, int version)
            : base(serviceUri, payloadFormat, version)
        {
        }

        [Fact]
        public async Task Success()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", CreateProduct(5001, "Test1"), false);
                await client.InsertEntryAsync("Products", CreateProduct(5002, "Test2"), false);
                await batch.CompleteAsync();
            }

            var product = await _client.FindEntryAsync("Products?$filter=Name eq 'Test1'");
            Assert.NotNull(product);
            product = await _client.FindEntryAsync("Products?$filter=Name eq 'Test2'");
            Assert.NotNull(product);
        }

        // OData.org sample service doesn't fail on this request
        //[Fact]
        //public async Task PartialFailures()
        //{
        //    using (var batch = new ODataBatch(_serviceUri))
        //    {
        //        var client = new ODataClient(batch);
        //        await client.InsertEntryAsync("Products", CreateProduct(5003, "Test3"), false);
        //        await client.InsertEntryAsync("Products", CreateProduct(0, "Test4"), false);
        //        await AssertThrowsAsync<WebRequestException>(async () => await batch.CompleteAsync());
        //    }
        //}

        // OData.org sample service doesn't fail on this request
        //[Fact]
        //public async Task AllFailures()
        //{
        //    using (var batch = new ODataBatch(_serviceUri))
        //    {
        //        var client = new ODataClient(batch);
        //        await client.InsertEntryAsync("Products", CreateProduct(0, "Test5"), false);
        //        await client.InsertEntryAsync("Products", CreateProduct(0, "Test6"), false);
        //        await AssertThrowsAsync<WebRequestException>(async () => await batch.CompleteAsync());
        //    }
        //}

        [Fact]
        public async Task InsertUpdateDeleteSingleBatch()
        {
            var key = new Entry() { { "Name", "Test11" } };

            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                await client.InsertEntryAsync("Products", CreateProduct(5011, "Test11"), false);
                await client.UpdateEntriesAsync("Products", "Products?$filter=Name eq 'Test11'", new Entry() { { "Price", 22m } });
                await client.DeleteEntriesAsync("Products", "Products?$filter=Name eq 'Test11'");
                await batch.CompleteAsync();
            }

            var product = await _client.FindEntryAsync("Products?$filter=Name eq 'Test11'");
            Assert.Equal(18d, Convert.ToDouble(product["Price"]));
        }

        // Not properly handled by OData.org sample service
        //[Fact]
        //public async Task InsertUpdateDeleteSeparateBatches()
        //{
        //    using (var batch = new ODataBatch(_serviceUri))
        //    {
        //        var client = new ODataClient(batch);
        //        await client.InsertEntryAsync("Products", CreateProduct(5012, "Test12"), false);
        //        await batch.CompleteAsync();
        //    }
        //    var product = await _client.FindEntryAsync("Products?$filter=Name eq 'Test12'");
        //    Assert.Equal(18d, Convert.ToDouble(product["Price"]));
        //    var key = new Entry() { { "ID", product["ID"] } };

        //    using (var batch = new ODataBatch(_serviceUri))
        //    {
        //        var client = new ODataClient(batch);
        //        await client.UpdateEntryAsync("Products", key, new Entry() { { "Price", 22m } });
        //        await batch.CompleteAsync();
        //    }
        //    product = await _client.FindEntryAsync("Products?$filter=Name eq 'Test12'");
        //    Assert.Equal(22d, Convert.ToDouble(product["Price"]));

        //    using (var batch = new ODataBatch(_serviceUri))
        //    {
        //        var client = new ODataClient(batch);
        //        await client.DeleteEntryAsync("Products", key);
        //        await batch.CompleteAsync();
        //    }
        //    product = await _client.FindEntryAsync("Products?$filter=Name eq 'Test12'");
        //    Assert.Null(product);
        //}

        [Fact]
        public async Task InsertSingleEntityWithSingleAssociationSingleBatch()
        {
            IDictionary<string, object> category;
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                category = await client.InsertEntryAsync("Categories", CreateCategory(5013, "Test13"));
                await client.InsertEntryAsync("Products", CreateProduct(5014, "Test14", category), false);
                await batch.CompleteAsync();
            }

            var product = await _client
                .For("Products")
                .Expand(ProductCategoryName)
                .Filter("Name eq 'Test14'")
                .FindEntryAsync();
            Assert.Equal(5013, ProductCategoryFunc(product)["ID"]);
        }

        [Fact]
        public async Task InsertSingleEntityWithMultipleAssociationsSingleBatch()
        {
            IDictionary<string, object> category;
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                var product1 = await client.InsertEntryAsync("Products", CreateProduct(5015, "Test15"));
                var product2 = await client.InsertEntryAsync("Products", CreateProduct(5016, "Test16"));
                await client.InsertEntryAsync("Categories", CreateCategory(5017, "Test17", new[] { product1, product2 } ), false);
                await batch.CompleteAsync();
            }

            category = await _client
                .For("Categories")
                .Key(5017)
                .Expand("Products")
                .FindEntryAsync();
            Assert.Equal(2, (category["Products"] as IEnumerable<object>).Count());
        }
    }
}
