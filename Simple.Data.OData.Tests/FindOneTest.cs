using System;
using Simple.OData.Client;
using Xunit;

namespace Simple.Data.OData.Tests
{
    public class FindOneTest : TestBase
    {
        [Fact]
        public void FindOne()
        {
            var product = _db.Products.FindByProductName("Chai");

            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void FindOneByKey()
        {
            var product = _db.Products.FindByProductID(1);

            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void FindNonExistingByKey()
        {
            Assert.Throws<WebRequestException>(() => _db.Products.FindByProductID(-1));
        }

        [Fact]
        public void FindNonExistingByKeyIgnoreException()
        {
            dynamic db = Database.Opener.Open(new ODataFeed { Url = _service.ServiceUri.AbsoluteUri, IgnoreResourceNotFoundException = true });
            var product = db.Products.FindByProductID(-1);

            Assert.Null(product);
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
            Assert.Throws<NotSupportedException>(
                () => _db.Products.Find(_db.Products.ProductName.InvalidFunction() == "Chai"));
        }
    }
}
