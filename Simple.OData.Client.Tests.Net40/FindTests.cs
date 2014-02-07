using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class FindTests : TestBase
    {
        [Fact]
        public void Filter()
        {
            var products = _client
                .For("Products")
                .Filter("ProductName eq 'Chai'")
                .FindEntries();
            Assert.Equal("Chai", products.Single()["ProductName"]);
        }

        [Fact]
        public void FilterStringExpression()
        {
            var products = _client
                .For("Products")
                .Filter("substringof('ai',ProductName)")
                .FindEntries();
            Assert.Equal("Chai", products.Single()["ProductName"]);
        }

        [Fact]
        public void Get()
        {
            var category = _client
                .For("Categories")
                .Key(1)
                .FindEntry();
            Assert.Equal(1, category["CategoryID"]);
        }

        [Fact]
        public void GetNonExisting()
        {
            Assert.Throws<WebRequestException>(() => _client
                .For("Categories")
                .Key(-1)
                .FindEntry());
        }

        [Fact]
        public void SkipOne()
        {
            var products = _client
                .For("Products")
                .Skip(1)
                .FindEntries();
            Assert.Equal(76, products.Count());
        }

        [Fact]
        public void TopOne()
        {
            var products = _client
                .For("Products")
                .Top(1)
                .FindEntries();
            Assert.Equal(1, products.Count());
        }

        [Fact]
        public void SkipOneTopOne()
        {
            var products = _client
                .For("Products")
                .Skip(1)
                .Top(1)
                .FindEntries();
            Assert.Equal(1, products.Count());
        }

        [Fact]
        public void OrderBy()
        {
            var product = _client
                .For("Products")
                .OrderBy("ProductName")
                .FindEntries().First();
            Assert.Equal("Alice Mutton", product["ProductName"]);
        }

        [Fact]
        public void OrderByDescending()
        {
            var product = _client
                .For("Products")
                .OrderByDescending("ProductName")
                .FindEntries().First();
            Assert.Equal("Zaanse koeken", product["ProductName"]);
        }

        [Fact]
        public void SelectSingle()
        {
            var product = _client
                .For("Products")
                .Select("ProductName")
                .FindEntry();
            Assert.Contains("ProductName", product.Keys);
            Assert.DoesNotContain("ProductID", product.Keys);
        }

        [Fact]
        public void SelectSingleHomogenize()
        {
            var product = _client
                .For("Products")
                .Select("Product_Name")
                .FindEntry();
            Assert.Contains("ProductName", product.Keys);
            Assert.DoesNotContain("ProductID", product.Keys);
        }

        [Fact]
        public void SelectMultiple()
        {
            var product = _client
                .For("Products")
                .Select("ProductID", "ProductName")
                .FindEntry();
            Assert.Contains("ProductName", product.Keys);
            Assert.Contains("ProductID", product.Keys);
        }

        [Fact]
        public void Expand()
        {
            var product = _client
                .For("Products")
                .OrderBy("ProductID")
                .Expand("Category")
                .FindEntries().Last();
            Assert.Equal("Condiments", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public void Count()
        {
            var count = _client
                .For("Products")
                .Count()
                .FindScalar();
            Assert.Equal(77, int.Parse(count.ToString()));
        }

        [Fact]
        public void FilterCount()
        {
            var count = _client
                .For("Products")
                .Filter("ProductName eq 'Chai'")
                .Count()
                .FindScalar();
            Assert.Equal(1, int.Parse(count.ToString()));
        }

        [Fact]
        public void TotalCount()
        {
            int count;
            var products = _client
                .For("Products")
                .FindEntries(true, out count);
            Assert.Equal(77, count);
            Assert.Equal(77, products.Count());
        }

        [Fact]
        public void CombineAll()
        {
            var product = _client
                .For("Products")
                .OrderBy("ProductName")
                .Skip(2)
                .Top(1)
                .Expand("Category")
                .Select("Category")
                .FindEntries().Single();
            Assert.Equal("Seafood", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public void CombineAllReverse()
        {
            var product = _client
                .For("Products")
                .Select("Category")
                .Expand("Category")
                .Top(1)
                .Skip(2)
                .OrderBy("ProductName")
                .FindEntries().Single();
            Assert.Equal("Seafood", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public void NavigateToSingle()
        {
            var category = _client
                .For("Products")
                .Key(new Entry() { { "ProductID", 2 } })
                .NavigateTo("Category")
                .FindEntry();
            Assert.Equal("Beverages", category["CategoryName"]);
        }

        [Fact]
        public void NavigateToMultiple()
        {
            var products = _client
                .For("Categories")
                .Key(2)
                .NavigateTo("Products")
                .FindEntries();
            Assert.Equal(12, products.Count());
        }

        [Fact]
        public void NavigateToRecursive()
        {
            var employee = _client
                .For("Employees")
                .Key(14)
                .NavigateTo("Superior")
                .NavigateTo("Superior")
                .NavigateTo("Subordinates")
                .Key(3)
                .FindEntry();
            Assert.Equal("Janet", employee["FirstName"]);
        }

        [Fact]
        public void BaseClassEntries()
        {
            var transport = _client
                .For("Transport")
                .FindEntries();
            Assert.Equal(2, transport.Count());
            Assert.False(transport.Any(x => x.ContainsKey(FluentCommand.ResourceTypeLiteral)));
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
                .For("Transport")
                .FindEntries();
            Assert.Equal(2, transport.Count());
            Assert.True(transport.All(x => x.ContainsKey(FluentCommand.ResourceTypeLiteral)));
        }

        [Fact]
        public void AllDerivedClassEntries()
        {
            var transport = _client
                .For("Transport")
                .As("Ships")
                .FindEntries();
            Assert.Equal("Titanic", transport.Single()["ShipName"]);
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
                .For("Transport")
                .As("Ships")
                .FindEntries();
            Assert.Equal("Titanic", transport.Single()["ShipName"]);
            Assert.Equal("Ships", transport.Single()[FluentCommand.ResourceTypeLiteral]);
        }

        [Fact]
        public void DerivedClassEntry()
        {
            var transport = _client
                .For("Transport")
                .As("Ships")
                .Filter("ShipName eq 'Titanic'")
                .FindEntry();
            Assert.Equal("Titanic", transport["ShipName"]);
        }

        [Fact]
        public void DerivedClassEntryBaseAndDerivedFields()
        {
            var transport = _client
                .For("Transport")
                .As("Ships")
                .Filter("TransportID eq 1 and ShipName eq 'Titanic'")
                .FindEntry();
            Assert.Equal("Titanic", transport["ShipName"]);
        }
    }
}
