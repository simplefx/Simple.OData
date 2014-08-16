using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace Simple.OData.Client.Tests
{
    public class ODataSchemaTests
    {
        private const string _serviceUrl = "http://services.odata.org/{0}/OData/OData.svc/";

        [Fact]
        public async Task ReadMetadataV3()
        {
            var client = new ODataClient(string.Format(_serviceUrl, "V3"));
            var metadata = await client.GetMetadataAsync<Microsoft.Data.Edm.IEdmModel>();
            Assert.Equal(12, metadata.SchemaElements.Count());
        }

        [Fact]
        public async Task ReadMetadataV3AsDynamic()
        {
            var client = new ODataClient(string.Format(_serviceUrl, "V3"));
            dynamic metadata = await client.GetMetadataAsync();
            Assert.Equal(12, (metadata.SchemaElements as IEnumerable<dynamic>).Count());
        }

        [Fact]
        public async Task ReadMetadataV4()
        {
            var client = new ODataClient(string.Format(_serviceUrl, "V4"));
            var metadata = await client.GetMetadataAsync<Microsoft.OData.Edm.IEdmModel>();
            Assert.Equal(12, metadata.SchemaElements.Count());
        }

        [Fact]
        public async Task ReadMetadataV4AsDynamic()
        {
            var client = new ODataClient(string.Format(_serviceUrl, "V4"));
            dynamic metadata = await client.GetMetadataAsync();
            Assert.Equal(12, (metadata.SchemaElements as IEnumerable<dynamic>).Count());
        }

        [Theory]
        [InlineData("V2", 3)]
        [InlineData("V3", 10)]
        [InlineData("V4", 10)]
        public async Task EntityTypes(string protocolVersion, int typeCount)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            Assert.Equal(typeCount, (await client.GetSchemaAsync()).EntityTypes.Count());
        }

        [Theory]
        [InlineData("V2")]
        [InlineData("V3")]
        [InlineData("V4")]
        public async Task ComplexTypes(string protocolVersion)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            Assert.Equal(1, (await client.GetSchemaAsync()).ComplexTypes.Count());
            Assert.Equal(5, (await client.GetSchemaAsync()).ComplexTypes.First().Properties.Count());
        }

        [Theory]
        [InlineData("V2")]
        [InlineData("V3")]
        [InlineData("V4")]
        public async Task ProductsPrimaryKey(string protocolVersion)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var table = (await client.GetSchemaAsync()).FindTable("Product");
            Assert.Equal("ID", table.PrimaryKey[0]);
        }

        [Theory]
        [InlineData("V2", "0..1")]
        [InlineData("V3", "*")]
        [InlineData("V4", "*")]
        public async Task ProductCategory(string protocolVersion, string mulitiplicity)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var table = (await client.GetSchemaAsync()).FindTable("Product");

            var association = table.FindAssociation("Categories");
            Assert.Equal("Category", association.ReferenceTableName);
            Assert.Equal(mulitiplicity, association.Multiplicity);
        }

        [Theory]
        [InlineData("V2")]
        [InlineData("V3")]
        [InlineData("V4")]
        public async Task GetProductsByRating(string protocolVersion)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var function = (await client.GetSchemaAsync()).FindFunction("GetProductsByRating");

            Assert.Equal("GetProductsByRating", function.ActualName);
            Assert.Equal("GET", function.HttpMethod);
            Assert.Equal(1, function.Parameters.Count);
            Assert.Equal("rating", function.Parameters[0]);
            Assert.Equal("Collection(Product)", function.ReturnType);
            Assert.Equal("Products", function.TableName);
        }
    }
}