using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class FindTypedTests : TestBase
    {
        [Fact]
        public async Task SingleCondition()
        {
            var product = await _client
                .For<Product>()
                .Filter(x => x.ProductName == "Chai")
                .FindEntryAsync();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public async Task SingleConditionWithLocalVariable()
        {
            var productName = "Chai";
            var product = await _client
                .For<Product>()
                .Filter(x => x.ProductName == productName)
                .FindEntryAsync();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public async Task SingleConditionWithMemberVariable()
        {
            var productName = "Chai";
            var product = await _client
                .For<Product>()
                .Filter(x => x.ProductName == productName)
                .FindEntryAsync();
            var sameProduct = await _client
                .For<Product>()
                .Filter(x => x.ProductName != product.ProductName)
                .FindEntryAsync();
            Assert.NotEqual(product.ProductName, sameProduct.ProductName);
        }

        [Fact]
        public async Task CombinedConditions()
        {
            var product = await _client
                .For<Employee>()
                .Filter(x => x.FirstName == "Nancy" && x.HireDate < DateTime.Now)
                .FindEntryAsync();
            Assert.Equal("Davolio", product.LastName);
        }

        [Fact]
        public async Task CombineAll()
        {
            var product = (await _client
                .For<Product>()
                .OrderBy(x => x.ProductName)
                .ThenByDescending(x => x.UnitPrice)
                .Skip(2)
                .Top(1)
                .Expand(x => x.Category)
                .Select(x => x.Category)
                .FindEntriesAsync()).Single();
            Assert.Equal("Seafood", product.Category.CategoryName);
        }

        [Fact]
        public async Task CombineAllReverse()
        {
            var product = (await _client
                .For<Product>()
                .Select(x => x.Category)
                .Expand(x => x.Category)
                .Top(1)
                .Skip(2)
                .OrderBy(x => x.ProductName)
                .ThenByDescending(x => x.UnitPrice)
                .FindEntriesAsync()).Single();
            Assert.Equal("Seafood", product.Category.CategoryName);
        }

        [Fact]
        public async Task StringContains()
        {
            var products = await _client
                .For<Product>()
                .Filter(x => x.ProductName.Contains("ai"))
                .FindEntriesAsync();
            Assert.Equal("Chai", products.Single().ProductName);
        }

        [Fact]
        public async Task StringContainsWithLocalVariable()
        {
            var text = "ai";
            var products = await _client
                .For<Product>()
                .Filter(x => x.ProductName.Contains(text))
                .FindEntriesAsync();
            Assert.Equal("Chai", products.Single().ProductName);
        }

        [Fact]
        public async Task StringNotContains()
        {
            var products = await _client
                .For<Product>()
                .Filter(x => !x.ProductName.Contains("ai"))
                .FindEntriesAsync();
            Assert.NotEqual("Chai", products.First().ProductName);
        }

        [Fact]
        public async Task StringStartsWith()
        {
            var products = await _client
                .For<Product>()
                .Filter(x => x.ProductName.StartsWith("Ch"))
                .FindEntriesAsync();
            Assert.Equal("Chai", products.First().ProductName);
        }

        [Fact]
        public async Task LengthOfStringEqual()
        {
            var products = await _client
                .For<Product>()
                .Filter(x => x.ProductName.Length == 4)
                .FindEntriesAsync();
            Assert.Equal("Chai", products.First().ProductName);
        }

        [Fact]
        public async Task SubstringWithPositionAndLengthEqual()
        {
            var products = await _client
                .For<Product>()
                .Filter(x => x.ProductName.Substring(1, 2) == "ha")
                .FindEntriesAsync();
            Assert.Equal("Chai", products.First().ProductName);
        }

        [Fact]
        public async Task SubstringWithPositionAndLengthEqualWithLocalVariable()
        {
            var text = "ha";
            var products = await _client
                .For<Product>()
                .Filter(x => x.ProductName.Substring(1, 2) == text)
                .FindEntriesAsync();
            Assert.Equal("Chai", products.First().ProductName);
        }

        [Fact]
        public async Task TopOne()
        {
            var products = await _client
                .For<Product>()
                .Filter(x => x.ProductName == "Chai")
                .Top(1)
                .FindEntriesAsync();
            Assert.Equal(1, products.Count());
        }

        [Fact]
        public async Task Count()
        {
            var count = await _client
                .For<Product>()
                .Filter(x => x.ProductName == "Chai")
                .Count()
                .FindScalarAsync();
            Assert.Equal(1, int.Parse(count.ToString()));
        }

        [Fact]
        public async Task Get()
        {
            var category = await _client
                .For<Category>()
                .Key(1)
                .FindEntryAsync();
            Assert.Equal(1, category.CategoryID);
        }

        [Fact]
        public async Task GetNonExisting()
        {
            await AssertThrowsAsync<WebRequestException>(async () => await _client
                .For<Category>()
                .Key(-1)
                .FindEntryAsync());
        }

        [Fact]
        public async Task SelectSingle()
        {
            var product = await _client
                .For<Product>()
                .Filter(x => x.ProductName == "Chai")
                .Select(x => x.ProductName)
                .FindEntryAsync();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public async Task SelectMultiple()
        {
            var product = await _client
                .For<Product>()
                .Filter(x => x.ProductName == "Chai")
                .Select(x => new { x.ProductID, x.ProductName })
                .FindEntryAsync();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public async Task OrderBySingle()
        {
            var product = await _client
                .For<Product>()
                .Filter(x => x.ProductName == "Chai")
                .OrderBy(x => x.ProductName)
                .FindEntryAsync();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public async Task OrderByMultiple()
        {
            var product = await _client
                .For<Product>()
                .Filter(x => x.ProductName == "Chai")
                .OrderBy(x => new { x.ProductID, x.ProductName })
                .FindEntryAsync();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public async Task NavigateToSingle()
        {
            var category = await _client
                .For<Product>()
                .Key(new { ProductID = 2 })
                .NavigateTo<Category>()
                .FindEntryAsync();
            Assert.Equal("Beverages", category.CategoryName);
        }

        [Fact]
        public async Task NavigateToSingleByExpression()
        {
            var category = await _client
                .For<Product>()
                .Key(new { ProductID = 2 })
                .NavigateTo(x => x.Category)
                .FindEntryAsync();
            Assert.Equal("Beverages", category.CategoryName);
        }

        [Fact]
        public async Task NavigateToMultiple()
        {
            var products = await _client
                .For<Category>()
                .Key(2)
                .NavigateTo<Product>()
                .FindEntriesAsync();
            Assert.Equal(12, products.Count());
        }

        [Fact]
        public async Task NavigateToRecursive()
        {
            var employee = await _client
                .For<Employee>()
                .Key(14)
                .NavigateTo<Employee>("Superior")
                .NavigateTo<Employee>("Superior")
                .NavigateTo<Employee>("Subordinates")
                .Key(3)
                .FindEntryAsync();
            Assert.Equal("Janet", employee.FirstName);
        }

        [Fact]
        public async Task NavigateToRecursiveByExpression()
        {
            var employee = await _client
                .For<Employee>()
                .Key(14)
                .NavigateTo(x => x.Superior)
                .NavigateTo(x => x.Superior)
                .NavigateTo(x => x.Subordinates)
                .Key(3)
                .FindEntryAsync();
            Assert.Equal("Janet", employee.FirstName);
        }

        [Fact]
        public async Task BaseClassEntries()
        {
            var transport = await _client
                .For<Transport>()
                .FindEntriesAsync();
            Assert.Equal(2, transport.Count());
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
                .For<Transport>()
                .FindEntriesAsync();
            Assert.Equal(2, transport.Count());
        }

        [Fact]
        public async Task AllDerivedClassEntries()
        {
            var transport = await _client
                .For<Transport>()
                .As<Ship>()
                .FindEntriesAsync();
            Assert.Equal("Titanic", transport.Single().ShipName);
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
                .For<Transport>()
                .As<Ship>()
                .FindEntriesAsync();
            Assert.Equal("Titanic", transport.Single().ShipName);
        }

        [Fact]
        public async Task DerivedClassEntry()
        {
            var transport = await _client
                .For<Transport>()
                .As<Ship>()
                .Filter(x => x.ShipName == "Titanic")
                .FindEntryAsync();
            Assert.Equal("Titanic", transport.ShipName);
        }

        [Fact]
        public async Task DerivedClassEntryBaseAndDerivedFields()
        {
            var transport = await _client
                .For<Transport>()
                .As<Ship>()
                .Filter(x => x.TransportID == 1 && x.ShipName == "Titanic")
                .FindEntryAsync();
            Assert.Equal("Titanic", transport.ShipName);
        }

        public class ODataOrgProduct
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        [Fact]
        public async Task TypedCombinedConditionsFromODataOrg()
        {
            var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");
            var product = await client
                .For<ODataOrgProduct>("Product")
                .Filter(x => x.Name == "Bread" && x.Price < 1000)
                .FindEntryAsync();
            Assert.Equal(2.5m, product.Price);
        }
    }
}
