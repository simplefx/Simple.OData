using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.OData;

namespace Simple.Data.OData.IntegrationTest
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
        public void FindWhereEqual()
        {
            var product = _db.Products.Find(_db.Products.ProductName == "Chai");

            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void FindWhereEqualToLower()
        {
            var product = _db.Products.Find(_db.Products.ProductName.ToLower() == "chai");

            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void FindWhereStartsWith()
        {
            var product = _db.Products.Find(_db.Products.ProductName.StartsWith("Ch") == true);

            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void FindWhereEqualWithInvalidFunction()
        {
            Assert.Throws<TableServiceException>(
                () => _db.Products.Find(_db.Products.ProductName.InvalidFunction() == "Chai"));
        }
    }
}
