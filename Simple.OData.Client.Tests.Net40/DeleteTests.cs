using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class DeleteTests : TestBase
    {
        [Fact]
        public async Task DeleteByKey()
        {
            var product = await _client
                .For("Products")
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Key(product["ProductID"])
                .DeleteEntryAsync();

            product = await _client
                .For("Products")
                .Filter("ProductName eq 'Test1'")
                .FindEntryAsync();

            Assert.Null(product);
        }

        [Fact]
        public async Task DeleteByKeyResetSchemaCache()
        {
            var product = await _client
                .For("Products")
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntryAsync();

            (_client as ODataClient).Session.ResetCache();
            await _client
                .For("Products")
                .Key(product["ProductID"])
                .DeleteEntryAsync();

            product = await _client
                .For("Products")
                .Filter("ProductName eq 'Test1'")
                .FindEntryAsync();

            Assert.Null(product);
        }

        [Fact]
        public async Task DeleteByFilter()
        {
            var product = await _client
                .For("Products")
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Filter("ProductName eq 'Test1'")
                .DeleteEntryAsync();

            product = await _client
                .For("Products")
                .Filter("ProductName eq 'Test1'")
                .FindEntryAsync();

            Assert.Null(product);
        }

        [Fact]
        public async Task DeleteByObjectAsKey()
        {
            var product = await _client
                .For("Products")
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Key(product)
                .DeleteEntryAsync();

            product = await _client
                .For("Products")
                .Filter("ProductName eq 'Test1'")
                .FindEntryAsync();

            Assert.Null(product);
        }
    }
}
