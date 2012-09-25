using Xunit;

namespace Simple.Data.OData.IntegrationTests
{
    public class DeleteTest : TestBase
    {
        [Fact]
        public void Delete()
        {
            var product = _db.Products.Insert(ProductName: "Test1", UnitPrice: 18m);
            product = _db.Products.FindByProductName("Test1");
            Assert.NotNull(product);

            _db.Products.Delete(ProductName: "Test1");

            product = _db.Products.FindByProductName("Test1");
            Assert.Null(product);
        }

        [Fact]
        public void DeleteObject()
        {
            var product = _db.Products.Insert(ProductName: "Test2", UnitPrice: 18m);
            product = _db.Products.FindByProductName("Test2");
            Assert.NotNull(product);

            _db.Products.Delete(product);

            product = _db.Products.FindByProductName("Test2");
            Assert.Null(product);
        }
    }
}
