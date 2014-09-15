using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class DeleteODataTestsV2Atom : DeleteODataTests
    {
        public DeleteODataTestsV2Atom() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Atom) { }
    }

    public class DeleteODataTestsV2Json : DeleteODataTests
    {
        public DeleteODataTestsV2Json() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public class DeleteODataTestsV3Atom : DeleteODataTests
    {
        public DeleteODataTestsV3Atom() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Atom) { }
    }

    public class DeleteODataTestsV3Json : DeleteODataTests
    {
        public DeleteODataTestsV3Json() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public class DeleteODataTestsV4Json : DeleteODataTests
    {
        public DeleteODataTestsV4Json() : base(ODataV4ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public abstract class DeleteODataTests : ODataTests
    {
        protected DeleteODataTests(string serviceUri, ODataPayloadFormat payloadFormat) : base(serviceUri, payloadFormat) { }

        [Fact]
        public async Task DeleteByKey()
        {
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Key(product["ID"])
                .DeleteEntryAsync();

            product = await _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .FindEntryAsync();

            Assert.Null(product);
        }

        [Fact]
        public async Task DeleteByKeyResetMetadataCache()
        {
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntryAsync();

            (_client as ODataClient).Session.ResetMetadataCache();
            await _client
                .For("Products")
                .Key(product["ID"])
                .DeleteEntryAsync();

            product = await _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .FindEntryAsync();

            Assert.Null(product);
        }

        [Fact]
        public async Task DeleteByFilter()
        {
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .DeleteEntryAsync();

            product = await _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .FindEntryAsync();

            Assert.Null(product);
        }

        [Fact]
        public async Task DeleteByObjectAsKey()
        {
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Key(product)
                .DeleteEntryAsync();

            product = await _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .FindEntryAsync();

            Assert.Null(product);
        }
    }
}
