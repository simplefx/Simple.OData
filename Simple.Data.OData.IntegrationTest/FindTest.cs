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
        public void FindFromStandardFeed()
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

        [Fact]
        public void FindFromFeedWithMediaLink()
        {
            var db = Database.Opener.Open("OData",
                new
                {
                    Url = "http://packages.nuget.org/v1/FeedService.svc/",
                });

            var package = db.Packages.FindByTitle("Simple.Data.Core");
            Assert.NotNull(package);
            Assert.Equal("Simple.Data.Core", package.Title);
        }
    }
}
