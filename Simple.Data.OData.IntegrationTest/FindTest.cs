using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTest
{
    using Xunit;

    public class FindTest
    {
        private const string _northwindUrl = "http://services.odata.org/Northwind/Northwind.svc/";
        private const string _nugetUrl = "http://packages.nuget.org/v1/FeedService.svc/";

        [Fact]
        public void FindOne()
        {
            var db = Database.Opener.Open(_northwindUrl);

            var product = db.Products.FindByProductName("Chai");
            Assert.NotNull(product);
            Assert.Equal("Chai", product.ProductName);
        }

        [Fact]
        public void FindFromFeedWithMediaLink()
        {
            var db = Database.Opener.Open(_nugetUrl);

            var package = db.Packages.FindByTitle("Simple.Data.Core");
            Assert.NotNull(package);
            Assert.Equal("Simple.Data.Core", package.Title);
        }

        [Fact]
        public void All()
        {
            var db = Database.Opener.Open(_northwindUrl);

            IEnumerable<dynamic> products = db.Products.All();
            Assert.NotNull(products);
            Assert.NotEmpty(products);
        }

        [Fact]
        public void CountAll()
        {
            var db = Database.Opener.Open(_northwindUrl);

            var count = db.Products.All().Count();
            Assert.True(count > 0);
        }

        [Fact]
        public void FindAll()
        {
            var db = Database.Opener.Open(_northwindUrl);

            IEnumerable<dynamic> products = db.Products.FindAllByProductName("Chai");
            Assert.NotNull(products);
            Assert.NotEmpty(products);
        }
    }
}
