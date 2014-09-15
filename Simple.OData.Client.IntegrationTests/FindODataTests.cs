using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class FindODataTestsV2Atom : FindODataTests
    {
        public FindODataTestsV2Atom() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Atom) { }
    }

    public class FindODataTestsV2Json : FindODataTests
    {
        public FindODataTestsV2Json() : base(ODataV2ReadWriteUri, ODataPayloadFormat.Json) { }
    }

    public class FindODataTestsV3Atom : FindODataTests
    {
        public FindODataTestsV3Atom() : base(ODataV3ReadOnlyUri, ODataPayloadFormat.Atom) { }
    }

    public class FindODataTestsV3Json : FindODataTests
    {
        public FindODataTestsV3Json() : base(ODataV3ReadOnlyUri, ODataPayloadFormat.Json) { }
    }

    public class FindODataTestsV4Json : FindODataTests
    {
        public FindODataTestsV4Json() : base(ODataV4ReadOnlyUri, ODataPayloadFormat.Json) { }
    }

    public abstract class FindODataTests : ODataTests
    {
        protected FindODataTests(string serviceUri, ODataPayloadFormat payloadFormat) : base(serviceUri, payloadFormat) { }

        [Fact]
        public async Task Filter()
        {
            var products = await _client
                .For("Products")
                .Filter("Name eq 'Chai'")
                .FindEntriesAsync();
            Assert.Equal("Chai", products.Single()["Name"]);
        }

        [Fact]
        public async Task FilterStringExpression()
        {
            var products = await _client
                .For("Products")
                .Filter("substringof('ai',Name)")
                .FindEntriesAsync();
            Assert.Equal("Chai", products.Single()["Name"]);
        }

        [Fact]
        public async Task Get()
        {
            var category = await _client
                .For("Categories")
                .Key(1)
                .FindEntryAsync();
            Assert.Equal(1, category["ID"]);
        }

        [Fact]
        public async Task GetNonExisting()
        {
            await AssertThrowsAsync<WebRequestException>(async () => await _client
                .For("Categories")
                .Key(-1)
                .FindEntryAsync());
        }

        [Fact]
        public async Task SkipOne()
        {
            var products = await _client
                .For("Products")
                .Skip(1)
                .FindEntriesAsync();
            Assert.Equal(20, products.Count());
        }

        [Fact]
        public async Task TopOne()
        {
            var products = await _client
                .For("Products")
                .Top(1)
                .FindEntriesAsync();
            Assert.Equal(1, products.Count());
        }

        [Fact]
        public async Task SkipOneTopOne()
        {
            var products = await _client
                .For("Products")
                .Skip(1)
                .Top(1)
                .FindEntriesAsync();
            Assert.Equal(1, products.Count());
        }

        [Fact]
        public async Task OrderBy()
        {
            var product = (await _client
                .For("Products")
                .OrderBy("Name")
                .FindEntriesAsync()).First();
            Assert.Equal("Alice Mutton", product["Name"]);
        }

        [Fact]
        public async Task OrderByDescending()
        {
            var product = (await _client
                .For("Products")
                .OrderByDescending("Name")
                .FindEntriesAsync()).First();
            Assert.Equal("Zaanse koeken", product["Name"]);
        }

        [Fact]
        public async Task SelectSingle()
        {
            var product = await _client
                .For("Products")
                .Select("Name")
                .FindEntryAsync();
            Assert.Contains("Name", product.Keys);
            Assert.DoesNotContain("ID", product.Keys);
        }

        [Fact]
        public async Task SelectSingleHomogenize()
        {
            var product = await _client
                .For("Products")
                .Select("Product_Name")
                .FindEntryAsync();
            Assert.Contains("Name", product.Keys);
            Assert.DoesNotContain("ID", product.Keys);
        }

        [Fact]
        public async Task SelectMultiple()
        {
            var product = await _client
                .For("Products")
                .Select("ID", "Name")
                .FindEntryAsync();
            Assert.Contains("Name", product.Keys);
            Assert.Contains("ID", product.Keys);
        }

        [Fact]
        public async Task ExpandOne()
        {
            var product = (await _client
                .For("Products")
                .OrderBy("ID")
                .Expand("Category")
                .FindEntriesAsync()).Last();
            Assert.Equal("Confections", (product["Category"] as IDictionary<string, object>)["Name"]);
        }

        [Fact]
        public async Task ExpandMany()
        {
            var category = await _client
                .For("Categories")
                .Expand("Products")
                .Filter("Name eq 'Beverages'")
                .FindEntryAsync();
            Assert.Equal(12, (category["Products"] as IEnumerable<object>).Count());
        }

        [Fact]
        public async Task ExpandSecondLevel()
        {
            var product = (await _client
                .For("Products")
                .OrderBy("ID")
                .Expand("Category/Products")
                .FindEntriesAsync()).Last();
            Assert.Equal(13, ((product["Category"] as IDictionary<string, object>)["Products"] as IEnumerable<object>).Count());
        }

        [Fact]
        public async Task Count()
        {
            var count = await _client
                .For("Products")
                .Count()
                .FindScalarAsync();
            Assert.Equal(77, int.Parse(count.ToString()));
        }

        [Fact]
        public async Task FilterCount()
        {
            var count = await _client
                .For("Products")
                .Filter("Name eq 'Chai'")
                .Count()
                .FindScalarAsync();
            Assert.Equal(1, int.Parse(count.ToString()));
        }

        [Fact]
        public async Task TotalCount()
        {
            var productsWithCount = await _client
                .For("Products")
                .FindEntriesWithCountAsync(true);
            Assert.Equal(77, productsWithCount.Item2);
            Assert.Equal(20, productsWithCount.Item1.Count());
        }

        [Fact]
        public async Task CombineAll()
        {
            var product = (await _client
                .For("Products")
                .OrderBy("Name")
                .Skip(2)
                .Top(1)
                .Expand("Category")
                .Select("Category")
                .FindEntriesAsync()).Single();
            Assert.Equal("Seafood", (product["Category"] as IDictionary<string, object>)["Name"]);
        }

        [Fact]
        public async Task CombineAllReverse()
        {
            var product = (await _client
                .For("Products")
                .Select("Category")
                .Expand("Category")
                .Top(1)
                .Skip(2)
                .OrderBy("Name")
                .FindEntriesAsync()).Single();
            Assert.Equal("Seafood", (product["Category"] as IDictionary<string, object>)["Name"]);
        }

        [Fact]
        public async Task NavigateToSingle()
        {
            var category = await _client
                .For("Products")
                .Key(new Dictionary<string, object>() { { "ID", 2 } })
                .NavigateTo("Category")
                .FindEntryAsync();
            Assert.Equal("Beverages", category["Name"]);
        }

        [Fact]
        public async Task NavigateToMultiple()
        {
            var products = await _client
                .For("Categories")
                .Key(2)
                .NavigateTo("Products")
                .FindEntriesAsync();
            Assert.Equal(12, products.Count());
        }
    }
}