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
            var products = _client
                .Collection("Products")
                .Skip(1)
                .FindEntries();
            Assert.Equal(76, products.Count());
        }

        [Fact]
        public void TopOne()
        {
            var products = _client
                .Collection("Products")
                .Top(1)
                .FindEntries();
            Assert.Equal(1, products.Count());
        }

        [Fact]
        public void SkipOneTopOne()
        {
            var products = _client
                .Collection("Products")
                .Skip(1)
                .Top(1)
                .FindEntries();
            Assert.Equal(1, products.Count());
        }

        [Fact]
        public void OrderBy()
        {
            var product = _client
                .Collection("Products")
                .OrderBy("ProductName")
                .FindEntries().First();
            Assert.Equal("Alice Mutton", product["ProductName"]);
        }

        [Fact]
        public void OrderByDescending()
        {
            var product = _client
                .Collection("Products")
                .OrderByDescending("ProductName")
                .FindEntries().First();
            Assert.Equal("Zaanse koeken", product["ProductName"]);
        }

        [Fact]
        public void SelectSingle()
        {
            var products = _client
                .Collection("Products")
                .Select("ProductName")
                .FindEntries();
            Assert.Contains("ProductName", products.First().Keys);
            Assert.DoesNotContain("ProductID", products.First().Keys);
        }

        [Fact]
        public void SelectMultiple()
        {
            var products = _client
                .Collection("Products")
                .Select("ProductID", "ProductName")
                .FindEntries();
            Assert.Contains("ProductName", products.First().Keys);
            Assert.Contains("ProductID", products.First().Keys);
        }

        [Fact]
        public void Expand()
        {
            var product = _client
                .Collection("Products")
                .OrderBy("ProductID")
                .Expand("Category")
                .FindEntries().Last();
            Assert.Equal("Condiments", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }

        [Fact]
        public void CombineAll()
        {
            var product = _client
                .Collection("Products")
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
                .Collection("Products")
                .Select("Category")
                .Expand("Category")
                .Top(1)
                .Skip(2)
                .OrderBy("ProductName")
                .FindEntries().Single();
            Assert.Equal("Seafood", (product["Category"] as IDictionary<string, object>)["CategoryName"]);
        }
    }
}
