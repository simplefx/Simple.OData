using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class ODataCommandTests : TestBase
    {
        [Fact]
        public void SkipOne()
        {
            var cmd = new ODataCommand("Products")
                .Skip(1);
            var products = _client.FindEntries(cmd);
            Assert.Equal(76, products.Count());
        }

        [Fact]
        public void TopOne()
        {
            var cmd = new ODataCommand("Products")
                .Top(1);
            var products = _client.FindEntries(cmd);
            Assert.Equal(1, products.Count());
        }

        [Fact]
        public void SkipOneTopOne()
        {
            var cmd = new ODataCommand("Products")
                .Skip(1)
                .Top(1);
            var products = _client.FindEntries(cmd);
            Assert.Equal(1, products.Count());
        }

        [Fact]
        public void OrderBy()
        {
            var cmd = new ODataCommand("Products")
                .OrderBy("ProductName");
            var product = _client.FindEntries(cmd).First();
            Assert.Equal("Alice Mutton", product["ProductName"]);
        }

        [Fact]
        public void OrderByDescending()
        {
            var cmd = new ODataCommand("Products")
                .OrderByDescending("ProductName");
            var product = _client.FindEntries(cmd).First();
            Assert.Equal("Zaanse koeken", product["ProductName"]);
        }

        [Fact]
        public void SelectSingle()
        {
            var cmd = new ODataCommand("Products")
                .Select("ProductName");
            var products = _client.FindEntries(cmd);
            Assert.Contains("ProductName", products.First().Keys);
            Assert.DoesNotContain("ProductID", products.First().Keys);
        }

        [Fact]
        public void SelectMultiple()
        {
            var cmd = new ODataCommand("Products")
                .Select("ProductID", "ProductName");
            var products = _client.FindEntries(cmd);
            Assert.Contains("ProductName", products.First().Keys);
            Assert.Contains("ProductID", products.First().Keys);
        }

        [Fact]
        public void Expand()
        {
            var cmd = new ODataCommand("Products")
                .OrderBy("ProductID")
                .Expand("Category");
            var product = _client.FindEntries(cmd).Last();
            Assert.Equal("Condiments", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public void CombineAll()
        {
            var cmd = new ODataCommand("Products")
                .OrderBy("ProductName")
                .Skip(2)
                .Top(1)
                .Expand("Category")
                .Select("Category");
            var product = _client.FindEntries(cmd).Single();
            Assert.Equal("Seafood", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public void CombineAllReverse()
        {
            var cmd = new ODataCommand("Products")
                .Select("Category")
                .Expand("Category")
                .Top(1)
                .Skip(2)
                .OrderBy("ProductName");
            var product = _client.FindEntries(cmd).Single();
            Assert.Equal("Seafood", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }
    }
}
