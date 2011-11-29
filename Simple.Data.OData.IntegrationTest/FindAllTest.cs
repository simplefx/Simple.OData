using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.OData;

namespace Simple.Data.OData.IntegrationTest
{
    using Xunit;

    public class FindAllTest : TestBase
    {
        [Fact]
        public void FindAll()
        {
            IEnumerable<dynamic> products = _db.Products.FindAllByProductName("Chai");

            Assert.NotEmpty(products);
        }

        [Fact]
        public void FindAllWithHomogenizedName()
        {
            IEnumerable<dynamic> products = _db.Products.FindAllByProduct_Name("Chai");

            Assert.NotEmpty(products);
        }

        [Fact]
        public void FindAllWithSpecificLength()
        {
            IEnumerable<dynamic> products = _db.Products.FindAll(_db.Products.ProductName.Length() == 4);

            Assert.NotEmpty(products);
        }

        [Fact]
        public void All()
        {
            IEnumerable<dynamic> products = _db.Products.All();

            Assert.NotEmpty(products);
        }

        [Fact]
        public void AllWithHomogenizedName()
        {
            IEnumerable<dynamic> orderDetails = _db.OrderDetails.All();

            Assert.NotEmpty(orderDetails);
        }

        [Fact]
        public void CountAll()
        {
            var count = _db.Products.All().Count();

            Assert.True(count > 0);
        }
    }
}
