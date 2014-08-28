using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Xunit;
using Moq;

namespace Simple.OData.Client.Tests
{
    public class FeedReaderTests : TestBase
    {
        private const int productProperties = 10;
        private const int categoryProperties = 4;

        [Fact]
        public async Task GetDataParsesSingleProduct()
        {
            string document = GetResourceAsString("SingleProduct.xml");
            var response = new Mock<IODataResponseMessageAsync>().Object;
            var responseReader = new ResponseReaderV3(response, await _client.GetMetadataAsync<IEdmModel>());
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(1, result.Count());
            Assert.Equal(productProperties, result.First().Count);
        }

        [Fact]
        public async Task GetDataParsesMultipleProducts()
        {
            string document = GetResourceAsString("MultipleProducts.xml");
            var response = new Mock<IODataResponseMessageAsync>().Object;
            var responseReader = new ResponseReaderV3(response, await _client.GetMetadataAsync<IEdmModel>());
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(20, result.Count());
            Assert.Equal(productProperties, result.First().Count);
        }

        [Fact]
        public async Task GetDataParsesSingleProductWithCategory()
        {
            string document = GetResourceAsString("SingleProductWithCategory.xml");
            var response = new Mock<IODataResponseMessageAsync>().Object;
            var responseReader = new ResponseReaderV3(response, await _client.GetMetadataAsync<IEdmModel>());
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(1, result.Count());
            Assert.Equal(productProperties + 1, result.First().Count);
            Assert.Equal(categoryProperties, (result.First()["Category"] as IDictionary<string, object>).Count);
        }

        [Fact]
        public async Task GetDataParsesMultipleProductsWithCategory()
        {
            string document = GetResourceAsString("MultipleProductsWithCategory.xml");
            var response = new Mock<IODataResponseMessageAsync>().Object;
            var responseReader = new ResponseReaderV3(response, await _client.GetMetadataAsync<IEdmModel>());
            var result = await responseReader.GetEntriesAsync();
            Assert.Equal(20, result.Count());
            Assert.Equal(productProperties + 1, result.First().Count);
            Assert.Equal(categoryProperties, (result.First()["Category"] as IDictionary<string, object>).Count);
        }

        [Fact]
        public async Task GetDataParsesSingleCategoryWithProducts()
        {
            string document = GetResourceAsString("SingleCategoryWithProducts.xml");
            var response = new Mock<IODataResponseMessageAsync>().Object;
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
            string document = GetResourceAsString("MultipleCategoriesWithProducts.xml");
            var response = new Mock<IODataResponseMessageAsync>().Object;
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
            string document = GetResourceAsString("SingleProductWithComplexProperty.xml");
            var response = new Mock<IODataResponseMessageAsync>().Object;
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
            string document = GetResourceAsString("SingleProductWithCollectionOfPrimitiveProperties.xml");
            var response = new Mock<IODataResponseMessageAsync>().Object;
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
            string document = GetResourceAsString("SingleProductWithCollectionOfComplexProperties.xml");
            var response = new Mock<IODataResponseMessageAsync>().Object;
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
            string document = GetResourceAsString("SingleProductWithEmptyCollectionOfComplexProperties.xml");
            var response = new Mock<IODataResponseMessageAsync>().Object;
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
            string document = GetResourceAsString("SingleCustomerWithAddress.xml");
            var response = new Mock<IODataResponseMessageAsync>().Object;
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
        //    string document = GetResourceAsString("Northwind.edmx");
        //    var schema = Schema.FromMetadata(document);
        //    var EntitySet = schema.FindEntitySet("Product");
        //    //var association = EntitySet.FindAssociation("OrderDetails");
        //    //Assert.NotNull(association);
        //}

        //[Fact]
        //public async Task GetArtifactsSchemaTableAssociations()
        //{
        //    string document = GetResourceAsString("Artifacts.edmx");
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

        private void ParseSchema(string schemaName)
        {
            var document = GetResourceAsString(schemaName + ".edmx");
            var metadata = ODataClient.ParseMetadataString<IEdmModel>(document);
            Assert.Equal(1, metadata.SchemaElements.Count(x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition));
            Assert.Equal(schemaName, metadata.SchemaElements.Single(x => x.SchemaElementKind == EdmSchemaElementKind.TypeDefinition).Name);
        }
    }
}
