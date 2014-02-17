using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class FindTypedTests : TestBase
    {
        [Fact]
        public void SingleCondition()
        {
            var product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Chai")
                .FindEntry();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void SingleConditionWithLocalVariable()
        {
            var productName = "Chai";
            var product = _client
                .For<Product>()
                .Filter(x => x.ProductName == productName)
                .FindEntry();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void SingleConditionWithMemberVariable()
        {
            var productName = "Chai";
            var product = _client
                .For<Product>()
                .Filter(x => x.ProductName == productName)
                .FindEntry();
            var sameProduct = _client
                .For<Product>()
                .Filter(x => x.ProductName != product.ProductName)
                .FindEntry();
            Assert.NotEqual(product.ProductName, sameProduct.ProductName);
        }

        [Fact]
        public void CombinedConditions()
        {
            var product = _client
                .For<Employee>()
                .Filter(x => x.FirstName == "Nancy" && x.HireDate < DateTime.Now)
                .FindEntry();
            Assert.Equal("Davolio", product.LastName);
        }

        [Fact]
        public void CombineAll()
        {
            var product = _client
                .For<Product>()
                .OrderBy(x => x.ProductName)
                .ThenByDescending(x => x.UnitPrice)
                .Skip(2)
                .Top(1)
                .Expand(x => x.Category)
                .Select(x => x.Category)
                .FindEntries().Single();
            Assert.Equal("Seafood", product.Category.CategoryName);
        }

        [Fact]
        public void CombineAllReverse()
        {
            var product = _client
                .For<Product>()
                .Select(x => x.Category)
                .Expand(x => x.Category)
                .Top(1)
                .Skip(2)
                .OrderBy(x => x.ProductName)
                .ThenByDescending(x => x.UnitPrice)
                .FindEntries().Single();
            Assert.Equal("Seafood", product.Category.CategoryName);
        }

        [Fact]
        public void StringContains()
        {
            var products = _client
                .For<Product>()
                .Filter(x => x.ProductName.Contains("ai"))
                .FindEntries();
            Assert.Equal("Chai", products.Single().ProductName);
        }

        [Fact]
        public void StringContainsWithLocalVariable()
        {
            var text = "ai";
            var products = _client
                .For<Product>()
                .Filter(x => x.ProductName.Contains(text))
                .FindEntries();
            Assert.Equal("Chai", products.Single().ProductName);
        }

        [Fact]
        public void StringNotContains()
        {
            var products = _client
                .For<Product>()
                .Filter(x => !x.ProductName.Contains("ai"))
                .FindEntries();
            Assert.NotEqual("Chai", products.First().ProductName);
        }

        [Fact]
        public void StringStartsWith()
        {
            var products = _client
                .For<Product>()
                .Filter(x => x.ProductName.StartsWith("Ch"))
                .FindEntries();
            Assert.Equal("Chai", products.First().ProductName);
        }

        [Fact]
        public void LengthOfStringEqual()
        {
            var products = _client
                .For<Product>()
                .Filter(x => x.ProductName.Length == 4)
                .FindEntries();
            Assert.Equal("Chai", products.First().ProductName);
        }

        [Fact]
        public void SubstringWithPositionAndLengthEqual()
        {
            var products = _client
                .For<Product>()
                .Filter(x => x.ProductName.Substring(1, 2) == "ha")
                .FindEntries();
            Assert.Equal("Chai", products.First().ProductName);
        }

        [Fact]
        public void SubstringWithPositionAndLengthEqualWithLocalVariable()
        {
            var text = "ha";
            var products = _client
                .For<Product>()
                .Filter(x => x.ProductName.Substring(1, 2) == text)
                .FindEntries();
            Assert.Equal("Chai", products.First().ProductName);
        }

        [Fact]
        public void TopOne()
        {
            var products = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Chai")
                .Top(1)
                .FindEntries();
            Assert.Equal(1, products.Count());
        }

        [Fact]
        public void Count()
        {
            var count = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Chai")
                .Count()
                .FindScalar();
            Assert.Equal(1, int.Parse(count.ToString()));
        }

        [Fact]
        public void Get()
        {
            var category = _client
                .For<Category>()
                .Key(1)
                .FindEntry();
            Assert.Equal(1, category.CategoryID);
        }

        [Fact]
        public void GetNonExisting()
        {
            Assert.Throws<WebRequestException>(() => _client
                .For<Category>()
                .Key(-1)
                .FindEntry());
        }

        [Fact]
        public void SelectSingle()
        {
            var product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Chai")
                .Select(x => x.ProductName)
                .FindEntry();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void SelectMultiple()
        {
            var product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Chai")
                .Select(x => new { x.ProductID, x.ProductName })
                .FindEntry();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void OrderBySingle()
        {
            var product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Chai")
                .OrderBy(x => x.ProductName)
                .FindEntry();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void OrderByMultiple()
        {
            var product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Chai")
                .OrderBy(x => new { x.ProductID, x.ProductName })
                .FindEntry();
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void NavigateToSingle()
        {
            var category = _client
                .For<Product>()
                .Key(new { ProductID = 2 })
                .NavigateTo<Category>()
                .FindEntry();
            Assert.Equal("Beverages", category.CategoryName);
        }

        [Fact]
        public void NavigateToSingleByExpression()
        {
            var category = _client
                .For<Product>()
                .Key(new { ProductID = 2 })
                .NavigateTo(x => x.Category)
                .FindEntry();
            Assert.Equal("Beverages", category.CategoryName);
        }

        [Fact]
        public void NavigateToMultiple()
        {
            var products = _client
                .For<Category>()
                .Key(2)
                .NavigateTo<Product>()
                .FindEntries();
            Assert.Equal(12, products.Count());
        }

        [Fact]
        public void NavigateToRecursive()
        {
            var employee = _client
                .For<Employee>()
                .Key(14)
                .NavigateTo<Employee>("Superior")
                .NavigateTo<Employee>("Superior")
                .NavigateTo<Employee>("Subordinates")
                .Key(3)
                .FindEntry();
            Assert.Equal("Janet", employee.FirstName);
        }

        [Fact]
        public void NavigateToRecursiveByExpression()
        {
            var employee = _client
                .For<Employee>()
                .Key(14)
                .NavigateTo(x => x.Superior)
                .NavigateTo(x => x.Superior)
                .NavigateTo(x => x.Subordinates)
                .Key(3)
                .FindEntry();
            Assert.Equal("Janet", employee.FirstName);
        }

        [Fact]
        public void BaseClassEntries()
        {
            var transport = _client
                .For<Transport>()
                .FindEntries();
            Assert.Equal(2, transport.Count());
        }

        [Fact]
        public void BaseClassEntriesWithResourceTypes()
        {
            var clientSettings = new ODataClientSettings
            {
                UrlBase = _serviceUri,
                IncludeResourceTypeInEntryProperties = true,
            };
            var client = new ODataClient(clientSettings);
            var transport = client
                .For<Transport>()
                .FindEntries();
            Assert.Equal(2, transport.Count());
        }

        [Fact]
        public void AllDerivedClassEntries()
        {
            var transport = _client
                .For<Transport>()
                .As<Ship>()
                .FindEntries();
            Assert.Equal("Titanic", transport.Single().ShipName);
        }

        [Fact]
        public void AllDerivedClassEntriesWithResourceTypes()
        {
            var clientSettings = new ODataClientSettings
            {
                UrlBase = _serviceUri,
                IncludeResourceTypeInEntryProperties = true,
            };
            var client = new ODataClient(clientSettings);
            var transport = client
                .For<Transport>()
                .As<Ship>()
                .FindEntries();
            Assert.Equal("Titanic", transport.Single().ShipName);
        }

        [Fact]
        public void DerivedClassEntry()
        {
            var transport = _client
                .For<Transport>()
                .As<Ship>()
                .Filter(x => x.ShipName == "Titanic")
                .FindEntry();
            Assert.Equal("Titanic", transport.ShipName);
        }

        [Fact]
        public void DerivedClassEntryBaseAndDerivedFields()
        {
            var transport = _client
                .For<Transport>()
                .As<Ship>()
                .Filter(x => x.TransportID == 1 && x.ShipName == "Titanic")
                .FindEntry();
            Assert.Equal("Titanic", transport.ShipName);
        }

        public class ODataOrgProduct
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        [Fact]
        public void TypedCombinedConditionsFromODataOrg()
        {
            var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");
            var product = client
                .For<ODataOrgProduct>("Product")
                .Filter(x => x.Name == "Bread" && x.Price < 1000)
                .FindEntry();
            Assert.Equal(2.5m, product.Price);
        }
    }
}
