using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class FindTests : TestBase
    {
        [Fact]
        public async Task Filter()
        {
            var products = await _client
                .For("Products")
                .Filter("ProductName eq 'Chai'")
                .FindEntriesAsync();
            Assert.Equal("Chai", products.Single()["ProductName"]);
        }

        [Fact]
        public async Task FilterStringExpression()
        {
            var products = await _client
                .For("Products")
                .Filter("substringof('ai',ProductName)")
                .FindEntriesAsync();
            Assert.Equal("Chai", products.Single()["ProductName"]);
        }

        [Fact]
        public async Task Get()
        {
            var category = await _client
                .For("Categories")
                .Key(1)
                .FindEntryAsync();
            Assert.Equal(1, category["CategoryID"]);
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
            Assert.Equal(76, products.Count());
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
                .OrderBy("ProductName")
                .FindEntriesAsync()).First();
            Assert.Equal("Alice Mutton", product["ProductName"]);
        }

        [Fact]
        public async Task OrderByDescending()
        {
            var product = (await _client
                .For("Products")
                .OrderByDescending("ProductName")
                .FindEntriesAsync()).First();
            Assert.Equal("Zaanse koeken", product["ProductName"]);
        }

        [Fact]
        public async Task SelectSingle()
        {
            var product = await _client
                .For("Products")
                .Select("ProductName")
                .FindEntryAsync();
            Assert.Contains("ProductName", product.Keys);
            Assert.DoesNotContain("ProductID", product.Keys);
        }

        [Fact]
        public async Task SelectSingleHomogenize()
        {
            var product = await _client
                .For("Products")
                .Select("Product_Name")
                .FindEntryAsync();
            Assert.Contains("ProductName", product.Keys);
            Assert.DoesNotContain("ProductID", product.Keys);
        }

        [Fact]
        public async Task SelectMultiple()
        {
            var product = await _client
                .For("Products")
                .Select("ProductID", "ProductName")
                .FindEntryAsync();
            Assert.Contains("ProductName", product.Keys);
            Assert.Contains("ProductID", product.Keys);
        }

        [Fact]
        public async Task ExpandOne()
        {
            var product = (await _client
                .For("Products")
                .OrderBy("ProductID")
                .Expand("Category")
                .FindEntriesAsync()).Last();
            Assert.Equal("Condiments", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public async Task ExpandMany()
        {
            var category = await _client
                .For("Categories")
                .Expand("Products")
                .Filter("CategoryName eq 'Beverages'")
                .FindEntryAsync();
            Assert.Equal(12, (category["Products"] as IEnumerable<object>).Count());
        }

        [Fact]
        public async Task ExpandODataOrg()
        {
            var client = new ODataClient("http://services.odata.org/V3/OData/OData.svc/");
            IEnumerable<object> productDetails = await client
                .For("Product")
                .Expand("ProductDetails")
                .Filter("ID eq 0")
                .FindEntriesAsync();
            Assert.NotEqual(0, productDetails.Count());
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
                .Filter("ProductName eq 'Chai'")
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
            Assert.Equal(77, productsWithCount.Item1.Count());
        }

        [Fact]
        public async Task CombineAll()
        {
            var product = (await _client
                .For("Products")
                .OrderBy("ProductName")
                .Skip(2)
                .Top(1)
                .Expand("Category")
                .Select("Category")
                .FindEntriesAsync()).Single();
            Assert.Equal("Seafood", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
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
                .OrderBy("ProductName")
                .FindEntriesAsync()).Single();
            Assert.Equal("Seafood", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public async Task NavigateToSingle()
        {
            var category = await _client
                .For("Products")
                .Key(new Entry() { { "ProductID", 2 } })
                .NavigateTo("Category")
                .FindEntryAsync();
            Assert.Equal("Beverages", category["CategoryName"]);
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

        [Fact]
        public async Task NavigateToRecursive()
        {
            var employee = await _client
                .For("Employees")
                .Key(14)
                .NavigateTo("Superior")
                .NavigateTo("Superior")
                .NavigateTo("Subordinates")
                .Key(3)
                .FindEntryAsync();
            Assert.Equal("Janet", employee["FirstName"]);
        }

        [Fact]
        public async Task BaseClassEntries()
        {
            var transport = await _client
                .For("Transport")
                .FindEntriesAsync();
            Assert.Equal(2, transport.Count());
            Assert.False(transport.Any(x => x.ContainsKey(FluentCommand.ResourceTypeLiteral)));
        }

        [Fact]
        public async Task BaseClassEntriesWithResourceTypes()
        {
            var clientSettings = new ODataClientSettings
                                     {
                                         UrlBase = _serviceUri,
                                         IncludeResourceTypeInEntryProperties = true,
                                     };
            var client = new ODataClient(clientSettings);
            var transport = await client
                .For("Transport")
                .FindEntriesAsync();
            Assert.Equal(2, transport.Count());
            Assert.True(transport.All(x => x.ContainsKey(FluentCommand.ResourceTypeLiteral)));
        }

        [Fact]
        public async Task AllDerivedClassEntries()
        {
            var transport = await _client
                .For("Transport")
                .As("Ships")
                .FindEntriesAsync();
            Assert.Equal("Titanic", transport.Single()["ShipName"]);
        }

        [Fact]
        public async Task AllDerivedClassEntriesWithResourceTypes()
        {
            var clientSettings = new ODataClientSettings
            {
                UrlBase = _serviceUri,
                IncludeResourceTypeInEntryProperties = true,
            };
            var client = new ODataClient(clientSettings);
            var transport = await client
                .For("Transport")
                .As("Ships")
                .FindEntriesAsync();
            Assert.Equal("Titanic", transport.Single()["ShipName"]);
            Assert.Equal("Ships", transport.Single()[FluentCommand.ResourceTypeLiteral]);
        }

        [Fact]
        public async Task DerivedClassEntry()
        {
            var transport = await _client
                .For("Transport")
                .As("Ships")
                .Filter("ShipName eq 'Titanic'")
                .FindEntryAsync();
            Assert.Equal("Titanic", transport["ShipName"]);
        }

        [Fact]
        public async Task DerivedClassEntryBaseAndDerivedFields()
        {
            var transport = await _client
                .For("Transport")
                .As("Ships")
                .Filter("TransportID eq 1 and ShipName eq 'Titanic'")
                .FindEntryAsync();
            Assert.Equal("Titanic", transport["ShipName"]);
        }
    }
}
