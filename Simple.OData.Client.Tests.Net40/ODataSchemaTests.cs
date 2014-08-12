using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace Simple.OData.Client.Tests
{
    public class ODataSchemaTests
    {
        private const string _serviceUrl = "http://services.odata.org/{0}/OData/OData.svc/";

        [Theory]
        [InlineData("V2", 3)]
        [InlineData("V3", 10)]
        //[InlineData("V4", 10)]
        public async Task EntityTypes(string protocolVersion, int typeCount)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            Assert.Equal(typeCount, (await client.GetSchemaAsync()).EntityTypes.Count());
        }

        [Theory]
        [InlineData("V2")]
        [InlineData("V3")]
        //[InlineData("V4")]
        public async Task ComplexTypes(string protocolVersion)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            Assert.Equal(1, (await client.GetSchemaAsync()).ComplexTypes.Count());
            Assert.Equal(5, (await client.GetSchemaAsync()).ComplexTypes.First().Properties.Count());
        }

        [Theory]
        [InlineData("V2")]
        [InlineData("V3")]
        //[InlineData("V4")]
        public async Task ProductsPrimaryKey(string protocolVersion)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var table = (await client.GetSchemaAsync()).FindTable("Product");
            Assert.Equal("ID", table.PrimaryKey[0]);
        }

        [Theory]
        [InlineData("V2", "0..1")]
        [InlineData("V3", "*")]
        //[InlineData("V4", "*")]
        public async Task ProductCategory(string protocolVersion, string mulitiplicity)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var table = (await client.GetSchemaAsync()).FindTable("Product");

            var association = table.FindAssociation("Categories");
            Assert.Equal("Categories", association.ReferenceTableName);
            Assert.Equal(mulitiplicity, association.Multiplicity);
        }

        [Theory]
        [InlineData("V2")]
        [InlineData("V3")]
        //[InlineData("V4")]
        public async Task GetProductsByRating(string protocolVersion)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var function = (await client.GetSchemaAsync()).FindFunction("GetProductsByRating");
            Assert.Equal("GET", function.HttpMethod);
            Assert.Equal("rating", function.Parameters[0]);
        }
    }
}