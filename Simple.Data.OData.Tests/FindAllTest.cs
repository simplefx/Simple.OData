using System.Collections.Generic;
using Simple.OData.Client;
using Xunit;

namespace Simple.Data.OData.Tests
{
    public class FindAllTest : TestBase
    {
        [Fact]
        public void FindAllByName()
        {
            IEnumerable<dynamic> products = _db.Products.FindAllByProductName("Chai");

            Assert.NotEmpty(products);
        }

        [Fact]
        public void FindAllByHomogenizedName()
        {
            IEnumerable<dynamic> products = _db.Products.FindAllByProduct_Name("Chai");

            Assert.NotEmpty(products);
        }

        [Fact]
        public void FindAllByNameWithSpecificLength()
        {
            IEnumerable<dynamic> products = _db.Products.FindAll(_db.Products.ProductName.Length() == 4);

            Assert.NotEmpty(products);
        }

        [Fact]
        public void FindAllByNameCount()
        {
            var count = _db.Products.FindAllByProductName("Chai").Count();

            Assert.Equal(1, count);
        }

        [Fact]
        public void FindAllByNameWithTotalCount()
        {
            Promise<int> count;
            IEnumerable<dynamic> products = _db.Products.FindAllByProductName("Chai").WithTotalCount(out count).Take(1);

            Assert.NotEmpty(products);
            Assert.Equal(1, count);
        }

        [Fact]
        public void FindAllByKey()
        {
            IEnumerable<dynamic> products = _db.Products.FindAllByProductID(1);

            Assert.NotEmpty(products);
        }

        [Fact]
        public void FindAllByNonExistingKey()
        {
            Assert.Throws<WebRequestException>(() => _db.Products.FindAllByProductID(-1).ToList());
        }

        [Fact]
        public void FindAllByNonExistingKeyIgnoreException()
        {
            dynamic db = Database.Opener.Open(new ODataFeed { Url = _service.ServiceUri.AbsoluteUri, IgnoreResourceNotFoundException = true });
            IEnumerable<dynamic> products = db.Products.FindAllByProductID(-1);

            Assert.Empty(products);
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
            IEnumerable<dynamic> orderDetails = _db.Order_Details.All();

            Assert.NotEmpty(orderDetails);
        }

        [Fact]
        public void AllCount()
        {
            var count = _db.Products.All().Count();

            Assert.True(count > 0);
        }

        [Fact]
        public void AllWithTotalCount()
        {
            Promise<int> count;
            IEnumerable<dynamic> products = _db.Products.All().WithTotalCount(out count).Take(1);

            Assert.NotEmpty(products);
            Assert.True(count > 1);
        }
    }
}
