using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class UpdateODataTestsV2Atom : UpdateODataTests
    {
        public UpdateODataTestsV2Atom() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Atom) { }
    }

    public class UpdateODataTestsV2Json : UpdateODataTests
    {
        public UpdateODataTestsV2Json() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public class UpdateODataTestsV3Atom : UpdateODataTests
    {
        public UpdateODataTestsV3Atom() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Atom) { }
    }

    public class UpdateODataTestsV3Json : UpdateODataTests
    {
        public UpdateODataTestsV3Json() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public class UpdateODataTestsV4Json : UpdateODataTests
    {
        public UpdateODataTestsV4Json() : base(ODataV4ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public abstract class UpdateODataTests : ODataTests
    {
        protected UpdateODataTests(string serviceUri, ODataPayloadFormat payloadFormat) : base(serviceUri, payloadFormat) { }

        [Fact]
        public async Task UpdateByKey()
        {
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Key(product["ID"])
                .Set(new {Price = 123m})
                .UpdateEntryAsync();

            product = await _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .FindEntryAsync();

            Assert.Equal(123m, product["Price"]);
        }

        [Fact]
        public async Task UpdateByKeyResetSchemaCache()
        {
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntryAsync();

            (_client as ODataClient).Session.ResetMetadataCache();
            await _client
                .For("Products")
                .Key(product["ID"])
                .Set(new { Price = 123m })
                .UpdateEntryAsync();

            product = await _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .FindEntryAsync();

            Assert.Equal(123m, product["Price"]);
        }

        [Fact]
        public async Task UpdateByFilter()
        {
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .Set(new { Price = 123m })
                .UpdateEntryAsync();

            product = await _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .FindEntryAsync();

            Assert.Equal(123m, product["Price"]);
        }

        [Fact]
        public async Task UpdateMultipleWithResult()
        {
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntryAsync();

            product = (await _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .Set(new { Price = 123m })
                .UpdateEntriesAsync()).Single();

            Assert.Equal(123m, product["Price"]);
        }

        [Fact]
        public async Task UpdateMultipleNoResult()
        {
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntryAsync();

            product = (await _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .Set(new { Price = 123m })
                .UpdateEntriesAsync(false)).Single();
            Assert.Null(product);

            product = await _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .FindEntryAsync();

            Assert.Equal(123m, product["Price"]);
        }

        [Fact]
        public async Task UpdateByObjectAsKey()
        {
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Key(product)
                .Set(new { Price = 456m })
                .UpdateEntryAsync();

            product = await _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .FindEntryAsync();

            Assert.Equal(456m, product["Price"]);
        }

        [Fact]
        public async Task UpdateDate()
        {
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);

            var employee = await _client
                .For("Employees")
                .Set(new { FirstName="Test1", LastName="Test1", HireDate = today })
                .InsertEntryAsync();

            await _client
                .For("Employees")
                .Key(employee["EmployeeID"])
                .Set(new { HireDate = tomorrow })
                .UpdateEntryAsync();

            employee = await _client
                .For("Employees")
                .Key(employee["EmployeeID"])
                .FindEntryAsync();

            Assert.Equal(tomorrow, employee["HireDate"]);
        }

        [Fact]
        public async Task AddSingleAssociation()
        {
            var category = await _client
                .For("Categories")
                .Set(new { Name = "Test1" })
                .InsertEntryAsync();
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test2", Price = 18m })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Key(product["ID"])
                .Set(new { Category = category })
                .UpdateEntryAsync();

            product = await _client
                .For("Products")
                .Filter("ID eq "+ product["ID"])
                .FindEntryAsync();
            Assert.Equal(category["CategoryID"], product["CategoryID"]);
            category = await _client
                .For("Categories")
                .Filter("CategoryID eq " + category["CategoryID"])
                .Expand("Products")
                .FindEntryAsync();
            Assert.Equal(1, (category["Products"] as IEnumerable<object>).Count());
        }

        [Fact]
        public async Task UpdateSingleAssociation()
        {
            var category = await _client
                .For("Categories")
                .Set(new { Name = "Test1" })
                .InsertEntryAsync();
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test2", Price = 18m, CategoryID = 1 })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Key(product["ID"])
                .Set(new { Category = category })
                .UpdateEntryAsync();

            product = await _client
                .For("Products")
                .Filter("ID eq " + product["ID"])
                .FindEntryAsync();
            Assert.Equal(category["CategoryID"], product["CategoryID"]);
            category = await _client
                .For("Categories")
                .Filter("CategoryID eq " + category["CategoryID"])
                .Expand("Products")
                .FindEntryAsync();
            Assert.Equal(1, (category["Products"] as IEnumerable<object>).Count());
        }

        [Fact]
        public async Task RemoveSingleAssociation()
        {
            var category = await _client
                .For("Categories")
                .Set(new { Name = "Test6" })
                .InsertEntryAsync();
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test7", Price = 18m, Category = category })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Key(product["ID"])
                .Set(new { Category = (int?)null })
                .UpdateEntryAsync();

            product = await _client
                .For("Products")
                .Filter("ID eq " + product["ID"])
                .FindEntryAsync();
            Assert.Null(product["CategoryID"]);
        }

        [Fact]
        public async Task UpdateMultipleAssociations()
        {
            var category = await _client
                .For("Categories")
                .Set(new { Name = "Test3" })
                .InsertEntryAsync();
            var product1 = await _client
                .For("Products")
                .Set(new { Name = "Test4", Price = 18m, CategoryID = 1 })
                .InsertEntryAsync();
            var product2 = await _client
                .For("Products")
                .Set(new { Name = "Test5", Price = 18m, CategoryID = 1 })
                .InsertEntryAsync();

            await _client
                .For("Categories")
                .Key(category["CategoryID"])
                .Set(new { Products = new[] { product1, product2 } })
                .UpdateEntryAsync();

            category = await _client
                .For("Categories")
                .Filter("CategoryID eq " + category["CategoryID"])
                .Expand("Products")
                .FindEntryAsync();
            Assert.Equal(2, (category["Products"] as IEnumerable<object>).Count());
        }
    }
}
