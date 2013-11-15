using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class FindDynamicTests : TestBase
    {
        [Fact]
        public void SingleCondition()
        {
            var x = ODataDynamic.Expression;
            var product = _client
                .For(x.Products)
                .Filter(x.ProductName == "Chai")
                .FindEntry();
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void SingleConditionWithVariable()
        {
            var productName = "Chai";
            var x = ODataDynamic.Expression;
            var product = _client
                .For(x.Products)
                .Filter(x.ProductName == productName)
                .FindEntry();
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void CombinedConditions()
        {
            var x = ODataDynamic.Expression;
            var product = _client
                .For(x.Employees)
                .Filter(x.FirstName == "Nancy" && x.HireDate < DateTime.Now)
                .FindEntry();
            Assert.Equal("Davolio", product["LastName"]);
        }

        [Fact]
        public void CombineAll()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For(x.Products)
                .OrderBy(x.ProductName)
                .ThenByDescending(x.UnitPrice)
                .Skip(2)
                .Top(1)
                .Expand(x.Category)
                .Select(x.Category)
                .FindEntries();
            Assert.Equal("Seafood", (products.Single()["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public void CombineAllReverse()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For(x.Products)
                .Select(x.Category)
                .Expand(x.Category)
                .Top(1)
                .Skip(2)
                .OrderBy(x.ProductName)
                .ThenByDescending(x.UnitPrice)
                .FindEntries();
            Assert.Equal("Seafood", (products.Single()["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public void StringContains()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For(x.Products)
                .Filter(x.ProductName.Contains("ai"))
                .FindEntries();
            Assert.Equal("Chai", products.Single()["ProductName"]);
        }

        [Fact]
        public void StringNotContains()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For(x.Products)
                .Filter(!x.ProductName.Contains("ai"))
                .FindEntries();
            Assert.NotEqual("Chai", products.First()["ProductName"]);
        }

        [Fact]
        public void StringStartsWith()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For(x.Products)
                .Filter(x.ProductName.StartsWith("Ch"))
                .FindEntries();
            Assert.Equal("Chai", products.First()["ProductName"]);
        }

        [Fact]
        public void LengthOfStringEqual()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For(x.Products)
                .Filter(x.ProductName.Length == 4)
                .FindEntries();
            Assert.Equal("Chai", products.First()["ProductName"]);
        }

        [Fact]
        public void SubstringWithPositionAndLengthEqual()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For(x.Products)
                .Filter(x.ProductName.Substring(1, 2) == "ha")
                .FindEntries();
            Assert.Equal("Chai", products.First()["ProductName"]);
        }

        [Fact]
        public void TopOne()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For(x.Products)
                .Filter(x.ProductName == "Chai")
                .Top(1)
                .FindEntries();
            Assert.Equal(1, products.Count());
        }

        [Fact]
        public void Count()
        {
            var x = ODataDynamic.Expression;
            var count = _client
                .For(x.Products)
                .Filter(x.ProductName == "Chai")
                .Count()
                .FindScalar();
            Assert.Equal(1, int.Parse(count.ToString()));
        }

        [Fact]
        public void SelectSingle()
        {
            var x = ODataDynamic.Expression;
            var product = _client
                .For(x.Products)
                .Filter(x.ProductName == "Chai")
                .Select(x.ProductName)
                .FindEntry();
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void SelectMultiple()
        {
            var x = ODataDynamic.Expression;
            var product = _client
                .For(x.Products)
                .Filter(x.ProductName == "Chai")
                .Select(x.ProductID, x.ProductName)
                .FindEntry();
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void OrderBySingle()
        {
            var x = ODataDynamic.Expression;
            var product = _client
                .For(x.Products)
                .Filter(x.ProductName == "Chai")
                .OrderBy(x.ProductName)
                .FindEntry();
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void OrderByMultiple()
        {
            var x = ODataDynamic.Expression;
            var product = _client
                .For(x.Products)
                .Filter(x.ProductName == "Chai")
                .OrderBy(x.ProductID, x.ProductName)
                .FindEntry();
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void NavigateToSingle()
        {
            var x = ODataDynamic.Expression;
            var category = _client
                .For(x.Products)
                .Key(2)
                .NavigateTo(x.Category)
                .FindEntry();
            Assert.Equal("Beverages", category["CategoryName"]);
        }

        [Fact]
        public void NavigateToMultiple()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For(x.Categories)
                .Key(2)
                .NavigateTo(x.Products)
                .FindEntries();
            Assert.Equal(12, products.Count());
        }

        [Fact]
        public void NavigateToRecursive()
        {
            var x = ODataDynamic.Expression;
            var employee = _client
                .For(x.Employees)
                .Key(14)
                .NavigateTo(x.Superior)
                .NavigateTo(x.Superior)
                .NavigateTo(x.Subordinates)
                .Key(3)
                .FindEntry();
            Assert.Equal("Janet", employee["FirstName"]);
        }

        [Fact]
        public void BaseClassEntries()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> transport = _client
                .For(x.Transport)
                .FindEntries();
            Assert.Equal(2, transport.Count());
            Assert.False(transport.Any(y => y.AsDictionary().ContainsKey(ODataCommand.ResourceTypeLiteral)));
        }

        [Fact]
        public void BaseClassEntriesWithResourceTypes()
        {
            var x = ODataDynamic.Expression;
            var clientSettings = new ODataClientSettings
            {
                UrlBase = _serviceUri,
                IncludeResourceTypeInEntryProperties = true,
            };
            var client = new ODataClient(clientSettings);
            IEnumerable<dynamic> transport = client
                .For(x.Transport)
                .FindEntries();
            Assert.Equal(2, transport.Count());
            Assert.True(transport.All(y => y.AsDictionary().ContainsKey(ODataCommand.ResourceTypeLiteral)));
        }

        [Fact]
        public void AllDerivedClassEntries()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> transport = _client
                .For(x.Transport)
                .As(x.Ships)
                .FindEntries();
            Assert.Equal("Titanic", transport.Single()["ShipName"]);
        }

        [Fact]
        public void AllDerivedClassEntriesWithResourceTypes()
        {
            var x = ODataDynamic.Expression;
            var clientSettings = new ODataClientSettings
            {
                UrlBase = _serviceUri,
                IncludeResourceTypeInEntryProperties = true,
            };
            var client = new ODataClient(clientSettings);
            IEnumerable<dynamic> transport = client
                .For(x.Transport)
                .As(x.Ships)
                .FindEntries();
            Assert.Equal("Titanic", transport.Single()["ShipName"]);
            Assert.Equal("Ships", transport.Single()[ODataCommand.ResourceTypeLiteral]);
        }

        [Fact]
        public void DerivedClassEntry()
        {
            var x = ODataDynamic.Expression;
            var transport = _client
                .For(x.Transport)
                .As(x.Ships)
                .Filter(x.ShipName == "Titanic")
                .FindEntry();
            Assert.Equal("Titanic", transport["ShipName"]);
        }

        [Fact]
        public void DerivedClassEntryBaseAndDerivedFields()
        {
            var x = ODataDynamic.Expression;
            var transport = _client
                .For(x.Transport)
                .As(x.Ships)
                .Filter(x.TransportID == 1 && x.ShipName == "Titanic")
                .FindEntry();
            Assert.Equal("Titanic", transport["ShipName"]);
        }

        [Fact]
        public void CombinedConditionsFromODataOrg()
        {
            var client = new ODataClient("http://services.odata.org/V3/OData/OData.svc/");
            var x = ODataDynamic.Expression;
            var product = client
                .For(x.Product)
                .Filter(x.Name == "Bread" && x.Price < 1000)
                .FindEntry();
            Assert.Equal(2.5m, product["Price"]);
        }
    }
}
