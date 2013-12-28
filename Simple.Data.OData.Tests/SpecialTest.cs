using Xunit;

namespace Simple.Data.OData.Tests
{
    public class SpecialTest
    {
        private const string _nugetUrl = "http://packages.nuget.org/v1/FeedService.svc/";
        private const string _odataOrgUrl = "http://services.odata.org/V2/OData/OData.svc/";

        public SpecialTest()
        {
        }

        [Fact]
        public void FindFromFeedWithMediaLink()
        {
            dynamic db = Database.Opener.Open(_nugetUrl);
            var package = db.Packages.FindByTitle("Simple.Data.Core");

            Assert.Equal("Simple.Data.Core", package.Title);
        }

        [Fact]
        public void FindByComplexTypeFromODataOrg()
        {
            dynamic db = Database.Opener.Open(_odataOrgUrl);
            var supplier = db.Suppliers.Find(db.Suppliers.Address.City == "Redmond");

            Assert.Equal("Redmond", supplier.Address.City);
        }
    }
}
