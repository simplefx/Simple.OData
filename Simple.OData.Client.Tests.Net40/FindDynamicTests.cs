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
                .For("Products")
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
                .For("Products")
                .Filter(x.ProductName == productName)
                .FindEntry();
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void CombinedConditions()
        {
            var x = ODataDynamic.Expression;
            var product = _client
                .For("Employees")
                .Filter(x.FirstName == "Nancy" && x.HireDate < DateTime.Now)
                .FindEntry();
            Assert.Equal("Davolio", product["LastName"]);
        }

        [Fact]
        public void StringContains()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For("Products")
                .Filter(x.ProductName.Contains("ai"))
                .FindEntries();
            Assert.Equal("Chai", products.Single()["ProductName"]);
        }

        [Fact]
        public void StringNotContains()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For("Products")
                .Filter(!x.ProductName.Contains("ai"))
                .FindEntries();
            Assert.NotEqual("Chai", products.First()["ProductName"]);
        }

        [Fact]
        public void StringStartsWith()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For("Products")
                .Filter(x.ProductName.StartsWith("Ch"))
                .FindEntries();
            Assert.Equal("Chai", products.First()["ProductName"]);
        }

        [Fact]
        public void LengthOfStringEqual()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For("Products")
                .Filter(x.ProductName.Length == 4)
                .FindEntries();
            Assert.Equal("Chai", products.First()["ProductName"]);
        }

        [Fact]
        public void SubstringWithPositionAndLengthEqual()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For("Products")
                .Filter(x.ProductName.Substring(1, 2) == "ha")
                .FindEntries();
            Assert.Equal("Chai", products.First()["ProductName"]);
        }

        [Fact]
        public void TopOne()
        {
            var x = ODataDynamic.Expression;
            IEnumerable<dynamic> products = _client
                .For("Products")
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
                .For("Products")
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
                .For("Products")
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
                .For("Products")
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
                .For("Products")
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
                .For("Products")
                .Filter(x.ProductName == "Chai")
                .OrderBy(x.ProductID, x.ProductName)
                .FindEntry();
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void CombinedConditionsFromODataOrg()
        {
            var client = new ODataClient("http://services.odata.org/V3/OData/OData.svc/");
            var x = ODataDynamic.Expression;
            var product = client
                .For("Product")
                .Filter(x.Name == "Bread" && x.Price < 1000)
                .FindEntry();
            Assert.Equal(2.5m, product["Price"]);
        }
    }
}
