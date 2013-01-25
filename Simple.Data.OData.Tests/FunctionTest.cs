using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Simple.Data.OData.Tests
{
    public class FunctionTest : TestBase
    {
        [Fact]
        public void ParseInt()
        {
            var result = _db.ParseInt(number: "1");

            Assert.Equal(1, result.ToScalarList()[0]);
        }

        [Fact]
        public void GetProductsByRatingWithNonEmptyResultSet()
        {
            var db = Database.Opener.Open("http://services.odata.org/OData/OData.svc/");
            IEnumerable<dynamic> products  = db.GetProductsByRating(rating: 3);

            Assert.NotEmpty(products);
            foreach (var product in products)
            {
                Assert.Equal(3, product.Rating);
            }
        }

        [Fact]
        public void GetProductsByRatingWithEmptyResultSet()
        {
            var db = Database.Opener.Open("http://services.odata.org/OData/OData.svc/");
            IEnumerable<dynamic> products = db.GetProductsByRating(rating: 999);

            Assert.Empty(products);
        }
    }
}