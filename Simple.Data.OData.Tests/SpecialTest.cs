using Xunit;

namespace Simple.Data.OData.Tests
{
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
