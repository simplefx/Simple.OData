using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class InsertODataTestsV2Atom : InsertODataTests
    {
        public InsertODataTestsV2Atom() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Atom) { }
    }

    public class InsertODataTestsV2Json : InsertODataTests
    {
        public InsertODataTestsV2Json() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public class InsertODataTestsV3Atom : InsertODataTests
    {
        public InsertODataTestsV3Atom() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Atom) { }
    }

    public class InsertODataTestsV3Json : InsertODataTests
    {
        public InsertODataTestsV3Json() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public class InsertODataTestsV4Json : InsertODataTests
    {
        public InsertODataTestsV4Json() : base(ODataV4ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public abstract class InsertODataTests : ODataTests
    {
        protected InsertODataTests(string serviceUri, ODataPayloadFormat payloadFormat) : base(serviceUri, payloadFormat) { }

        [Fact]
        public async Task Insert()
        {
            var product = await _client
                .For("Products")
                .Set(CreateProduct(1001, "Test1"))
                .InsertEntryAsync();

            Assert.Equal("Test1", product["Name"]);
        }

        [Fact]
        public async Task InsertLookupByID()
        {
            var product = await _client
                .For("Products")
                .Set(CreateProduct(1002, "Test1"))
                .InsertEntryAsync();

            Assert.True((int)product["ID"] > 0);
            Assert.Equal("Test1", product["Name"]);
        }

        [Fact]
        public async Task InsertExpando()
        {
            dynamic expando = new ExpandoObject();
            expando.ID = 1003;
            expando.Name = "Test9";
            expando.Description = "Test9";
            expando.Price = 18m;
            expando.ReleaseDate = DateTime.Now;

            var product = await (Task<IDictionary<string, object>>)_client
                .For("Products")
                .Set(expando)
                .InsertEntryAsync();

            Assert.True((int)product["ID"] > 0);
        }

        [Fact]
        public async Task InsertProductWithCategoryByID()
        {
            var category = await _client
                .For("Categories")
                .Set(CreateCategory(1004, "Test4"))
                .InsertEntryAsync();
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test4", Price = 18m, CategoryID = category["CategoryID"] })
                .InsertEntryAsync();

            Assert.Equal("Test4", product["Name"]);
            Assert.Equal(category["CategoryID"], product["CategoryID"]);
            category = await _client
                .For("Categories")
                .Expand("Products")
                .Filter("CategoryName eq 'Test4'")
                .FindEntryAsync();
            Assert.True((category["Products"] as IEnumerable<object>).Count() == 1);
        }

        [Fact]
        public async Task InsertProductWithCategoryByAssociation()
        {
            var category = await _client
                .For("Categories")
                .Set(CreateCategory(1005, "Test5"))
                .InsertEntryAsync();
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test6", Price = 18m, Category = category })
                .InsertEntryAsync();

            Assert.Equal("Test6", product["Name"]);
            Assert.Equal(category["CategoryID"], product["CategoryID"]);
            category = await _client
                .For("Categories")
                .Expand("Products")
                .Filter("CategoryName eq 'Test5'")
                .FindEntryAsync();
            Assert.True((category["Products"] as IEnumerable<object>).Count() == 1);
        }

        private Entry CreateProduct(int productId, string productName)
        {
            return new Entry()
            {
                {"ID", productId},
                {"Name", productName},
                {"Description", "Test1"},
                {"Price", 18},
                {"Rating", 1},
                {"ReleaseDate", DateTimeOffset.Now},
            };
        }

        private Entry CreateCategory(int categoryId, string categoryName)
        {
            return new Entry()
            {
                {"ID", categoryId},
                {"Name", categoryName},
            };
        }
    }
}
