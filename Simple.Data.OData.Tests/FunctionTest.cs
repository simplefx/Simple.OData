using System;
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

            Assert.Equal(1, result.ToScalarArray()[0]);
        }

        [Fact]
        public void ReturnIntCollectionSingleElement()
        {
            var result = _db.ReturnIntCollection(count: 1);

            Assert.Equal(1, result.ToScalarArray()[0]);
        }

        [Fact]
        public void ReturnIntCollectionMultipeElements()
        {
            var result = _db.ReturnIntCollection(count: 3);

            Assert.Equal(1, result.ToScalarArray()[0]);
            Assert.Equal(2, result.ToScalarArray()[1]);
            Assert.Equal(3, result.ToScalarArray()[2]);
        }

        [Fact]
        public void PassThroughLong()
        {
            var result = _db.PassThroughLong(number: 1L);

            Assert.Equal(1L, result.ToScalarArray()[0]);
        }

        [Fact]
        public void PassThroughDateTime()
        {
            var dateTime = new DateTime(2013, 1, 1, 12, 13, 14);
            var result = _db.PassThroughDateTime(dateTime: dateTime);

            Assert.Equal(dateTime.ToLocalTime(), result.ToScalarArray()[0]);
        }

        [Fact]
        public void PassThroughGuid()
        {
            var guid = Guid.NewGuid();
            var result = _db.PassThroughGuid(guid: guid);

            Assert.Equal(guid, result.ToScalarArray()[0]);
        }

        [Fact]
        public void ReturnAddressCollectionSingleElement()
        {
            var result = _db.ReturnAddressCollection(count: 1);

            Assert.Equal("Oslo", result.ToScalarArray()[0]["City"]);
            Assert.Equal("Norway", result.ToScalarArray()[0]["Country"]);
        }

        [Fact]
        public void ReturnAddressCollectionMultipleElements()
        {
            var result = _db.ReturnAddressCollection(count: 3);

            Assert.Equal("Oslo", result.ToScalarArray()[0]["City"]);
            Assert.Equal("Norway", result.ToScalarArray()[0]["Country"]);
            Assert.Equal("Oslo", result.ToScalarArray()[1]["City"]);
            Assert.Equal("Oslo", result.ToScalarArray()[2]["City"]);
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