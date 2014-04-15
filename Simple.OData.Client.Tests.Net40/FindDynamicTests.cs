using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
#if !NET40
    public class FindDynamicTests : TestBase
    {
        [Fact]
        public async Task SingleCondition()
        {
            var x = ODataDynamic.Expression;
            var product = await _client
                .For(x.Products)
                .Filter(x.ProductName == "Chai")
                .FindEntryAsync();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public async Task SingleConditionWithVariable()
        {
            var productName = "Chai";
            var x = ODataDynamic.Expression;
            var product = await _client
                .For(x.Products)
                .Filter(x.ProductName == productName)
                .FindEntryAsync();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public async Task CombinedConditions()
        {
            var x = ODataDynamic.Expression;
            var product = await _client
                .For(x.Employees)
                .Filter(x.FirstName == "Nancy" && x.HireDate < DateTime.Now)
                .FindEntryAsync();
            Assert.Equal("Davolio", product.LastName);
        }

        [Fact]
        public async Task CombineAll()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = await _client
                .For(x.Products)
                .OrderBy(x.ProductName)
                .ThenByDescending(x.UnitPrice)
                .Skip(2)
                .Top(1)
                .Expand(x.Category)
                .Select(x.Category)
                .FindEntriesAsync();
            Assert.Equal("Seafood", products.Single().Category.CategoryName);
        }

        [Fact]
        public async Task CombineAllReverse()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = await _client
                .For(x.Products)
                .Select(x.Category)
                .Expand(x.Category)
                .Top(1)
                .Skip(2)
                .OrderBy(x.ProductName)
                .ThenByDescending(x.UnitPrice)
                .FindEntriesAsync();
            Assert.Equal("Seafood", products.Single().Category.CategoryName);
        }

        [Fact]
        public async Task StringContains()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = await _client
                .For(x.Products)
                .Filter(x.ProductName.Contains("ai"))
                .FindEntriesAsync();
            Assert.Equal("Chai", products.Single().ProductName);
        }

        [Fact]
        public async Task StringContainsWithLocalVariable()
        {
            var text = "ai";
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = await _client
                .For(x.Products)
                .Filter(x.ProductName.Contains(text))
                .FindEntriesAsync();
            Assert.Equal("Chai", products.Single().ProductName);
        }

        [Fact]
        public async Task StringNotContains()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = await _client
                .For(x.Products)
                .Filter(!x.ProductName.Contains("ai"))
                .FindEntriesAsync();
            Assert.NotEqual("Chai", products.First().ProductName);
        }

        [Fact]
        public async Task StringStartsWith()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = await _client
                .For(x.Products)
                .Filter(x.ProductName.StartsWith("Ch"))
                .FindEntriesAsync();
            Assert.Equal("Chai", products.First().ProductName);
        }

        [Fact]
        public async Task LengthOfStringEqual()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = await _client
                .For(x.Products)
                .Filter(x.ProductName.Length == 4)
                .FindEntriesAsync();
            Assert.Equal("Chai", products.First().ProductName);
        }

        [Fact]
        public async Task SubstringWithPositionAndLengthEqual()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = await _client
                .For(x.Products)
                .Filter(x.ProductName.Substring(1, 2) == "ha")
                .FindEntriesAsync();
            Assert.Equal("Chai", products.First().ProductName);
        }

        [Fact]
        public async Task SubstringWithPositionAndLengthEqualWithLocalVariable()
        {
            var text = "ha";
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = await _client
                .For(x.Products)
                .Filter(x.ProductName.Substring(1, 2) == text)
                .FindEntriesAsync();
            Assert.Equal("Chai", products.First().ProductName);
        }

        [Fact]
        public async Task TopOne()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = await _client
                .For(x.Products)
                .Filter(x.ProductName == "Chai")
                .Top(1)
                .FindEntriesAsync();
            Assert.Equal(1, products.Count());
        }

        [Fact]
        public async Task Count()
        {
            var x = ODataDynamic.Expression;
            var count = await _client
                .For(x.Products)
                .Filter(x.ProductName == "Chai")
                .Count()
                .FindScalarAsync();
            Assert.Equal(1, int.Parse(count.ToString()));
        }

        [Fact]
        public async Task Get()
        {
            var x = ODataDynamic.Expression;
            var category = await _client
                .For(x.Categories)
                .Key(1)
                .FindEntryAsync();
            Assert.Equal(1, category.CategoryID);
        }

        [Fact]
        public async Task GetNonExisting()
        {
            var x = ODataDynamic.Expression;
            await AssertThrowsAsync<WebRequestException>(async () => await _client
                .For(x.Categories)
                .Key(-1)
                .FindEntryAsync());
        }

        [Fact]
        public async Task SelectSingle()
        {
            var x = ODataDynamic.Expression;
            var product = await _client
                .For(x.Products)
                .Filter(x.ProductName == "Chai")
                .Select(x.ProductName)
                .FindEntryAsync();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public async Task SelectMultiple()
        {
            var x = ODataDynamic.Expression;
            var product = await _client
                .For(x.Products)
                .Filter(x.ProductName == "Chai")
                .Select(x.ProductID, x.ProductName)
                .FindEntryAsync();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public async Task OrderBySingle()
        {
            var x = ODataDynamic.Expression;
            var product = await _client
                .For(x.Products)
                .Filter(x.ProductName == "Chai")
                .OrderBy(x.ProductName)
                .FindEntryAsync();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public async Task OrderByMultiple()
        {
            var x = ODataDynamic.Expression;
            var product = await _client
                .For(x.Products)
                .Filter(x.ProductName == "Chai")
                .OrderBy(x.ProductID, x.ProductName)
                .FindEntryAsync();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public async Task NavigateToSingle()
        {
            var x = ODataDynamic.Expression;
            var category = await _client
                .For(x.Products)
                .Key(2)
                .NavigateTo(x.Category)
                .FindEntryAsync();
            Assert.Equal("Beverages", category.CategoryName);
        }

        [Fact]
        public async Task NavigateToMultiple()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = await _client
                .For(x.Categories)
                .Key(2)
                .NavigateTo(x.Products)
                .FindEntriesAsync();
            Assert.Equal(12, products.Count());
        }

        [Fact]
        public async Task NavigateToRecursive()
        {
            var x = ODataDynamic.Expression;
            var employee = await _client
                .For(x.Employees)
                .Key(14)
                .NavigateTo(x.Superior)
                .NavigateTo(x.Superior)
                .NavigateTo(x.Subordinates)
                .Key(3)
                .FindEntryAsync();
            Assert.Equal("Janet", employee.FirstName);
        }

        [Fact]
        public async Task BaseClassEntries()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> transport = await _client
                .For(x.Transport)
                .FindEntriesAsync();
            Assert.Equal(2, transport.Count());
            Assert.False(transport.Any(y => y.AsDictionary().ContainsKey(FluentCommand.ResourceTypeLiteral)));
        }

        [Fact]
        public async Task BaseClassEntriesWithResourceTypes()
        {
            var x = ODataDynamic.Expression;
            var clientSettings = new ODataClientSettings
            {
                UrlBase = _serviceUri,
                IncludeResourceTypeInEntryProperties = true,
            };
            var client = new ODataClient(clientSettings);
            IEnumerable<dynamic> transport = await client
                .For(x.Transport)
                .FindEntriesAsync();
            Assert.Equal(2, transport.Count());
            Assert.True(transport.All(y => y.AsDictionary().ContainsKey(FluentCommand.ResourceTypeLiteral)));
        }

        [Fact]
        public async Task AllDerivedClassEntries()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> transport = await _client
                .For(x.Transport)
                .As(x.Ships)
                .FindEntriesAsync();
            Assert.Equal("Titanic", transport.Single().ShipName);
        }

        [Fact]
        public async Task AllDerivedClassEntriesWithResourceTypes()
        {
            var x = ODataDynamic.Expression;
            var clientSettings = new ODataClientSettings
            {
                UrlBase = _serviceUri,
                IncludeResourceTypeInEntryProperties = true,
            };
            var client = new ODataClient(clientSettings);
            var transport = await client
                .For(x.Transport)
                .As(x.Ships)
                .FindEntriesAsync();
            Assert.Equal("Titanic", (transport as IEnumerable<dynamic>).Single().ShipName);
            Assert.Equal("Ships", (transport as IEnumerable<dynamic>).Single()[FluentCommand.ResourceTypeLiteral]);
        }

        [Fact]
        public async Task DerivedClassEntry()
        {
            var x = ODataDynamic.Expression;
            var transport = await _client
                .For(x.Transport)
                .As(x.Ships)
                .Filter(x.ShipName == "Titanic")
                .FindEntryAsync();
            Assert.Equal("Titanic", transport.ShipName);
        }

        [Fact]
        public async Task DerivedClassEntryBaseAndDerivedFields()
        {
            var x = ODataDynamic.Expression;
            var transport = await _client
                .For(x.Transport)
                .As(x.Ships)
                .Filter(x.TransportID == 1 && x.ShipName == "Titanic")
                .FindEntryAsync();
            Assert.Equal("Titanic", transport.ShipName);
        }

        [Fact]
        public async Task CombinedConditionsFromODataOrg()
        {
            var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");
            var x = ODataDynamic.Expression;
            var product = await client
                .For(x.Product)
                .Filter(x.Name == "Bread" && x.Price < 1000)
                .FindEntryAsync();
            Assert.Equal(2.5m, product.Price);
        }
    }
#endif
}
