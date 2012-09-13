using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTests
{
    using Xunit;

    public class DeleteTest : TestBase
    {
        [Fact]
        public void Delete()
        {
            var product = _db.Products.Insert(ProductName: "Test", UnitPrice: 18m);
            product = _db.Products.FindByProductName("Chai");
            Assert.NotNull(product);

            _db.Products.Delete(ProductName: "Test");

            product = _db.Products.FindByProductName("Test");
            Assert.Null(product);
        }
    }
}
