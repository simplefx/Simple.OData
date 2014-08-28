using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Xunit;
using Moq;

namespace Simple.OData.Client.Tests
{
    public class ResponseReaderTests : TestBase
    {
        private const int productProperties = 10;
        private const int categoryProperties = 4;

        [Fact]
        public async Task GetDataParsesSingleProduct()
        {
            var response = SetUpMock("SingleProduct.xml");
            var responseReader = new ResponseReaderV3(response, null);
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(1, result.Count());
            Assert.Equal(productProperties, result.First().Count);
        }

        [Fact]
        public async Task GetDataParsesMultipleProducts()
        {
            var response = SetUpMock("MultipleProducts.xml");
            var responseReader = new ResponseReaderV3(response, await _client.GetMetadataAsync<IEdmModel>());
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(20, result.Count());
            Assert.Equal(productProperties, result.First().Count);
        }

        [Fact]
        public async Task GetDataParsesSingleProductWithCategory()
        {
            var response = SetUpMock("SingleProductWithCategory.xml");
            var responseReader = new ResponseReaderV3(response, await _client.GetMetadataAsync<IEdmModel>());
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(1, result.Count());
            Assert.Equal(productProperties + 1, result.First().Count);
            Assert.Equal(categoryProperties, (result.First()["Category"] as IDictionary<string, object>).Count);
        }

        [Fact]
        public async Task GetDataParsesMultipleProductsWithCategory()
        {
            var response = SetUpMock("MultipleProductsWithCategory.xml");
            var responseReader = new ResponseReaderV3(response, await _client.GetMetadataAsync<IEdmModel>());
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(20, result.Count());
            Assert.Equal(productProperties + 1, result.First().Count);
            Assert.Equal(categoryProperties, (result.First()["Category"] as IDictionary<string, object>).Count);
        }

        [Fact]
        public async Task GetDataParsesSingleCategoryWithProducts()
        {
            var response = SetUpMock("SingleCategoryWithProducts.xml");
            var responseReader = new ResponseReaderV3(response, await _client.GetMetadataAsync<IEdmModel>());
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(1, result.Count());
            Assert.Equal(categoryProperties + 1, result.First().Count);
            Assert.Equal(12, (result.First()["Products"] as IEnumerable<IDictionary<string, object>>).Count());
            Assert.Equal(productProperties,
                         (result.First()["Products"] as IEnumerable<IDictionary<string, object>>).First().Count);
        }

        [Fact]
        public async Task GetDataParsesMultipleCategoriesWithProducts()
        {
            var response = SetUpMock("MultipleCategoriesWithProducts.xml");
            var responseReader = new ResponseReaderV3(response, await _client.GetMetadataAsync<IEdmModel>());
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(8, result.Count());
            Assert.Equal(categoryProperties + 1, result.First().Count);
            Assert.Equal(12, (result.First()["Products"] as IEnumerable<IDictionary<string, object>>).Count());
            Assert.Equal(productProperties,
                         (result.First()["Products"] as IEnumerable<IDictionary<string, object>>).First().Count);
        }

        [Fact]
        public async Task GetDataParsesSingleProductWithComplexProperty()
        {
            var response = SetUpMock("SingleProductWithComplexProperty.xml");
            var responseReader = new ResponseReaderV3(response, await _client.GetMetadataAsync<IEdmModel>());
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(1, result.Count());
            Assert.Equal(productProperties + 1, result.First().Count);
            var quantity = result.First()["Quantity"] as IDictionary<string, object>;
            Assert.NotNull(quantity);
            Assert.Equal(10d, quantity["Value"]);
            Assert.Equal("bags", quantity["Units"]);
        }

        [Fact]
        public async Task GetDataParsesSingleProductWithCollectionOfPrimitiveProperties()
        {
            var response = SetUpMock("SingleProductWithCollectionOfPrimitiveProperties.xml");
            var responseReader = new ResponseReaderV3(response, await _client.GetMetadataAsync<IEdmModel>());
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(1, result.Count());
            Assert.Equal(productProperties + 2, result.First().Count);
            var tags = result.First()["Tags"] as IList<dynamic>;
            Assert.Equal(2, tags.Count);
            Assert.Equal("Bakery", tags[0]);
            Assert.Equal("Food", tags[1]);
            var ids = result.First()["Ids"] as IList<dynamic>;
            Assert.Equal(2, ids.Count);
            Assert.Equal(1, ids[0]);
            Assert.Equal(2, ids[1]);
        }

        [Fact]
        public async Task GetDataParsesSingleProductWithCollectionOfComplexProperties()
        {
            var response = SetUpMock("SingleProductWithCollectionOfComplexProperties.xml");
            var responseReader = new ResponseReaderV3(response, await _client.GetMetadataAsync<IEdmModel>());
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(1, result.Count());
            Assert.Equal(productProperties + 1, result.First().Count);
            var tags = result.First()["Tags"] as IList<dynamic>;
            Assert.Equal(2, tags.Count);
            Assert.Equal("Food", tags[0]["group"]);
            Assert.Equal("Bakery", tags[0]["value"]);
            Assert.Equal("Food", tags[1]["group"]);
            Assert.Equal("Meat", tags[1]["value"]);
        }

        [Fact]
        public async Task GetDataParsesSingleProductWithEmptyCollectionOfComplexProperties()
        {
            var response = SetUpMock("SingleProductWithEmptyCollectionOfComplexProperties.xml");
            var responseReader = new ResponseReaderV3(response, await _client.GetMetadataAsync<IEdmModel>());
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(1, result.Count());
            Assert.Equal(productProperties + 1, result.First().Count);
            var tags = result.First()["Tags"] as IList<dynamic>;
            Assert.Equal(0, tags.Count);
        }

        [Fact]
        public async Task GetDataParsesSingleCustomerWithAddress()
        {
            var response = SetUpMock("SingleCustomerWithAddress.xml");
            var responseReader = new ResponseReaderV3(response, await _client.GetMetadataAsync<IEdmModel>());
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(1, result.Count());
            Assert.Equal(3, result.First().Count);
            Assert.Equal(5, (result.First()["Address"] as IEnumerable<KeyValuePair<string, object>>).Count());
            Assert.Equal("Private", ((result.First()["Address"] as IEnumerable<KeyValuePair<string, object>>)).First().Value);
        }

        //[Fact]
        //public async Task GetNorthwindSchemaTableAssociations()
        //{
        //    var response = SetUpMock("Northwind.edmx");
        //    var schema = Schema.FromMetadata(document);
        //    var EntitySet = schema.FindEntitySet("Product");
        //    //var association = EntitySet.FindAssociation("OrderDetails");
        //    //Assert.NotNull(association);
        //}

        //[Fact]
        //public async Task GetArtifactsSchemaTableAssociations()
        //{
        //    var response = SetUpMock("Artifacts.edmx");
        //    var schema = Schema.FromMetadata(document);
        //    var EntitySet = schema.FindEntitySet("Product");
        //    //var association = EntitySet.FindAssociation("Artifacts");
        //    Assert.NotNull(association);
        //}

        [Fact]
        public async Task GetColorsSchema()
        {
            ParseSchema("Colors");
        }

        [Fact]
        public async Task GetFacebookSchema()
        {
            ParseSchema("Facebook");
        }

        [Fact]
        public async Task GetFlickrSchema()
        {
            ParseSchema("Flickr");
        }

        [Fact]
        public async Task GetGoogleMapsSchema()
        {
            ParseSchema("GoogleMaps");
        }

        [Fact]
        public async Task GetiPhoneSchema()
        {
            ParseSchema("iPhone");
        }

        [Fact]
        public async Task GetTwitterSchema()
        {
            ParseSchema("Twitter");
        }

        [Fact]
        public async Task GetYouTubeSchema()
        {
            ParseSchema("YouTube");
        }

        [Fact]
        public async Task GetNestedSchema()
        {
            ParseSchema("Nested");
        }

        [Fact]
        public async Task GetArrayOfNestedSchema()
        {
            ParseSchema("ArrayOfNested");
        }

        private IODataResponseMessageAsync SetUpMock(string resourceName)
        {
            var document = GetResourceAsString(resourceName);
            var mock = new Mock<IODataResponseMessageAsync>();
            mock.Setup(x => x.GetStreamAsync()).ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes(document)));
            mock.Setup(x => x.GetStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes(document)));
            mock.Setup(x => x.GetHeader("Content-Type")).Returns(() => "application/atom+xml; type=feed; charset=utf-8");
            var response = mock.Object;
            return response;
        }

        private void ParseSchema(string schemaName)
        {
            var document = GetResourceAsString(schemaName + ".edmx");
            var metadata = ODataClient.ParseMetadataString<IEdmModel>(document);
            Assert.Equal(1, metadata.SchemaElements.Count(x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition));
            Assert.Equal(schemaName, metadata.SchemaElements.Single(x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition).Name);
        }
    }
}
