using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTest
{
    using Xunit;

    public class FindTest
    {
        [Fact]
        public void SimpleFindTest()
        {
            var db = Database.Opener.Open("OData",
                new
                {
                    Url = "http://services.odata.org/Northwind/Northwind.svc/",
                });

            var product = db.Products.FindByProductName("Chai");
            Assert.NotNull(product);
            Assert.Equal("Chai", product.ProductName);
        }
    }
}
