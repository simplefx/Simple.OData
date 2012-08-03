using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.OData;

namespace Simple.Data.OData.IntegrationTest
{
    using Xunit;

    public class SpecialTest
    {
        private const string _nugetUrl = "http://packages.nuget.org/v1/FeedService.svc/";
        dynamic _db;

        public SpecialTest()
        {
            _db = Database.Opener.Open(_nugetUrl);
        }

        [Fact]
        public void FindFromFeedWithMediaLink()
        {
            var package = _db.Packages.FindByTitle("Simple.Data.Core");

            Assert.Equal("Simple.Data.Core", package.Title);
        }
    }
}
