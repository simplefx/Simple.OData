using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTest
{
    using Xunit;

    public class InsertTest : TestBase
    {
        [Fact]
        public void Insert()
        {
            var product = _db.Products.Insert(ProductID: 1001, ProductName: "Test", UnitPrice: 18m);

            Assert.Equal("Test", product.ProductName);
        }
    }
}
