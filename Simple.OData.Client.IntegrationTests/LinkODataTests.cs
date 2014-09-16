using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class LinkODataTestsV2Atom : LinkODataTests
    {
        public LinkODataTestsV2Atom() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Atom) { }
    }

    public class LinkODataTestsV2Json : LinkODataTests
    {
        public LinkODataTestsV2Json() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public class LinkODataTestsV3Atom : LinkODataTests
    {
        public LinkODataTestsV3Atom() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Atom) { }
    }

    public class LinkODataTestsV3Json : LinkODataTests
    {
        public LinkODataTestsV3Json() : base(ODataV3ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public class LinkODataTestsV4Json : LinkODataTests
    {
        public LinkODataTestsV4Json() : base(ODataV4ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public abstract class LinkODataTests : ODataTests
    {
        protected LinkODataTests(string serviceUri, ODataPayloadFormat payloadFormat) : base(serviceUri, payloadFormat) { }

        [Fact]
        public async Task LinkEntry()
        {
            var category = await _client
                .For("Categories")
                .Set(new { Name = "Test4" })
                .InsertEntryAsync();
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test5" })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Key(product)
                .LinkEntryAsync("Category", category);

            product = await _client
                .For("Products")
                .Filter("Name eq 'Test5'")
                .FindEntryAsync();
            Assert.NotNull(product["CategoryID"]);
            Assert.Equal(category["ID"], product["CategoryID"]);
        }

        [Fact]
        public async Task UnlinkEntry()
        {
            var category = await _client
                .For("Categories")
                .Set(new { Name = "Test4" })
                .InsertEntryAsync();
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test5", CategoryID = category["CategoryID"] })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Key(product)
                .UnlinkEntryAsync("Category");

            product = await _client
                .For("Products")
                .Filter("Name eq 'Test5'")
                .FindEntryAsync();
            Assert.Null(product["CategoryID"]);
        }
    }
}
