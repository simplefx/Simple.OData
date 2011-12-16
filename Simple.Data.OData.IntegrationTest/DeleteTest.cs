using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTest
{
    using Xunit;

    public class DeleteTest : TestBase
    {
        [Fact]
        public void Delete()
        {
            var product = _db.Products.FindByProductName("Chai");
            Assert.NotNull(product);

            _db.Products.Delete(ProductName: "Chai");

            product = _db.Products.FindByProductName("Chai");
            Assert.Null(product);
        }
    }
}
