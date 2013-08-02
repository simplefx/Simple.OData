using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
#if !PORTABLE_IOS
    public class FindDynamicFilterTests : TestBase
    {
        [Fact]
        public void SingleCondition()
        {
            var x = ODataFilter.Expression;
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
            var x = ODataFilter.Expression;
            var product = _client
                .For("Products")
                .Filter(x.ProductName == productName)
                .FindEntry();
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void CombinedConditions()
        {
            var x = ODataFilter.Expression;
            var product = _client
                .For("Employees")
                .Filter(x.FirstName == "Nancy" && x.HireDate < DateTime.Now)
                .FindEntry();
            Assert.Equal("Davolio", product["LastName"]);
        }

        [Fact]
        public void TopOne()
        {
            var x = ODataFilter.Expression;
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
            var x = ODataFilter.Expression;
            var count = _client
                .For("Products")
                .Filter(x.ProductName == "Chai")
                .Count()
                .FindScalar();
            Assert.Equal(1, int.Parse(count.ToString()));
        }
    }
#endif
}
