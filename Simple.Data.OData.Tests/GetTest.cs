using Xunit;

namespace Simple.Data.OData.Tests
{
    public class GetTest : TestBase
    {
        [Fact]
        public void GetBySingleNumericKey()
        {
            var product = _db.Products.Get(1);

            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void GetBySingleStringKey()
        {
            var customer = _db.Customers.Get("ALFKI");

            Assert.Equal("ALFKI", customer.CustomerID);
        }

        [Fact]
        public void GetByCompoundKey()
        {
            var orderDetails = _db.OrderDetails.Get(10248, 11);

            Assert.Equal(10248, orderDetails.OrderID);
        }
    }
}
