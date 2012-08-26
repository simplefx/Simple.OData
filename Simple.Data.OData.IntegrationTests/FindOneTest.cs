using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.OData;

namespace Simple.Data.OData.IntegrationTests
{
    using Xunit;

    public class FindOneTest : TestBase
    {
        [Fact]
        public void FindOne()
        {
            var product = _db.Products.FindByProductName("Chai");

            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void FindWhereNameEqual()
        {
            var product = _db.Products.Find(_db.Products.ProductName == "Chai");

            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void FindWhereNameEqualToLower()
        {
            var product = _db.Products.Find(_db.Products.ProductName.ToLower() == "chai");

            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void FindWhereNameWithSpecificLength()
        {
            var product = _db.Products.Find(_db.Products.ProductName.Length() == 4);

            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void FindWhereNameStartsWith()
        {
            var product = _db.Products.Find(_db.Products.ProductName.StartsWith("Ch") == true);

            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void FindWhereNameContainsEqualsTrue()
        {
            var product = _db.Products.Find(_db.Products.ProductName.Contains("ai") == true);

            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void FindWhereNameContainsEqualsFalse()
        {
            var product = _db.Products.Find(_db.Products.ProductName.Contains("ai") == false);

            Assert.Equal("Chang", product.ProductName);
        }

        [Fact]
        public void FindWhereNameIndexOf()
        {
            var product = _db.Products.Find(_db.Products.ProductName.IndexOf("ai") == 2);

            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void FindWhereNameEqualSubstring()
        {
            var product = _db.Products.Find(_db.Products.ProductName.Substring(1) == "hai");

            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void FindWhereNameEqualWithInvalidFunction()
        {
            Assert.Throws<TableServiceException>(
                () => _db.Products.Find(_db.Products.ProductName.InvalidFunction() == "Chai"));
        }
    }
}
