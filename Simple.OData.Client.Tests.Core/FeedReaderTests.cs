using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class FeedReaderTests : TestBase
    {
        private readonly ResponseReader _feedReader;
        private const int productProperties = 10;
        private const int categoryProperties = 4;

        public FeedReaderTests()
        {
            _feedReader = new ResponseReader(_client.GetSchemaAsync().Result);
        }

        [Fact]
        public void GetDataParsesSingleProduct()
        {
            string document = GetResourceAsString("SingleProduct.xml");
            var result = _feedReader.GetData(document);
            Assert.Equal(1, result.Count());
            Assert.Equal(productProperties, result.First().Count);
        }

        [Fact]
        public void GetDataParsesMultipleProducts()
        {
            string document = GetResourceAsString("MultipleProducts.xml");
            var result = _feedReader.GetData(document);
            Assert.Equal(20, result.Count());
            Assert.Equal(productProperties, result.First().Count);
        }

        [Fact]
        public void GetDataParsesSingleProductWithCategory()
        {
            string document = GetResourceAsString("SingleProductWithCategory.xml");
            var result = _feedReader.GetData(document);
            Assert.Equal(1, result.Count());
            Assert.Equal(productProperties + 1, result.First().Count);
            Assert.Equal(categoryProperties, (result.First()["Category"] as IDictionary<string, object>).Count);
        }

        [Fact]
        public void GetDataParsesMultipleProductsWithCategory()
        {
            string document = GetResourceAsString("MultipleProductsWithCategory.xml");
            var result = _feedReader.GetData(document);
            Assert.Equal(20, result.Count());
            Assert.Equal(productProperties + 1, result.First().Count);
            Assert.Equal(categoryProperties, (result.First()["Category"] as IDictionary<string, object>).Count);
        }

        [Fact]
        public void GetDataParsesSingleCategoryWithProducts()
        {
            string document = GetResourceAsString("SingleCategoryWithProducts.xml");
            var result = _feedReader.GetData(document);
            Assert.Equal(1, result.Count());
            Assert.Equal(categoryProperties + 1, result.First().Count);
            Assert.Equal(12, (result.First()["Products"] as IEnumerable<IDictionary<string, object>>).Count());
            Assert.Equal(productProperties,
                         (result.First()["Products"] as IEnumerable<IDictionary<string, object>>).First().Count);
        }

        [Fact]
        public void GetDataParsesMultipleCategoriesWithProducts()
        {
            string document = GetResourceAsString("MultipleCategoriesWithProducts.xml");
            var result = _feedReader.GetData(document);
            Assert.Equal(8, result.Count());
            Assert.Equal(categoryProperties + 1, result.First().Count);
            Assert.Equal(12, (result.First()["Products"] as IEnumerable<IDictionary<string, object>>).Count());
            Assert.Equal(productProperties,
                         (result.First()["Products"] as IEnumerable<IDictionary<string, object>>).First().Count);
        }

        [Fact]
        public void GetDataParsesSingleProductWithComplexProperty()
        {
            string document = GetResourceAsString("SingleProductWithComplexProperty.xml");
            var result = _feedReader.GetData(document);
            Assert.Equal(1, result.Count());
            Assert.Equal(productProperties + 1, result.First().Count);
            var quantity = result.First()["Quantity"] as IDictionary<string, object>;
            Assert.NotNull(quantity);
            Assert.Equal(10d, quantity["Value"]);
            Assert.Equal("bags", quantity["Units"]);
        }

        [Fact]
        public void GetDataParsesSingleProductWithCollectionOfPrimitiveProperties()
        {
            string document = GetResourceAsString("SingleProductWithCollectionOfPrimitiveProperties.xml");
            var result = _feedReader.GetData(document);
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
        public void GetDataParsesSingleProductWithCollectionOfComplexProperties()
        {
            string document = GetResourceAsString("SingleProductWithCollectionOfComplexProperties.xml");
            var result = _feedReader.GetData(document);
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
        public void GetDataParsesSingleProductWithEmptyCollectionOfComplexProperties()
        {
            string document = GetResourceAsString("SingleProductWithEmptyCollectionOfComplexProperties.xml");
            var result = _feedReader.GetData(document);
            Assert.Equal(1, result.Count());
            Assert.Equal(productProperties + 1, result.First().Count);
            var tags = result.First()["Tags"] as IList<dynamic>;
            Assert.Equal(0, tags.Count);
        }

        [Fact]
        public void GetNorthwindSchemaTableAssociations()
        {
            string document = GetResourceAsString("Northwind.edmx");
            var schema = Schema.FromMetadata(document);
            var table = schema.FindTable("Product");
            var association = table.FindAssociation("OrderDetails");
            Assert.NotNull(association);
        }

        [Fact]
        public void GetArtifactsSchemaTableAssociations()
        {
            string document = GetResourceAsString("Artifacts.edmx");
            var schema = Schema.FromMetadata(document);
            var table = schema.FindTable("Product");
            var association = table.FindAssociation("Artifacts");
            Assert.NotNull(association);
        }

        [Fact]
        public void GetColorsSchema()
        {
            ParseSchema("Colors");
        }

        [Fact]
        public void GetFacebookSchema()
        {
            ParseSchema("Facebook");
        }

        [Fact]
        public void GetFlickrSchema()
        {
            ParseSchema("Flickr");
        }

        [Fact]
        public void GetGoogleMapsSchema()
        {
            ParseSchema("GoogleMaps");
        }

        [Fact]
        public void GetiPhoneSchema()
        {
            ParseSchema("iPhone");
        }

        [Fact]
        public void GetTwitterSchema()
        {
            ParseSchema("Twitter");
        }

        [Fact]
        public void GetYouTubeSchema()
        {
            ParseSchema("YouTube");
        }

        [Fact]
        public void GetNestedSchema()
        {
            ParseSchema("Nested");
        }

        [Fact]
        public void GetArrayOfNestedSchema()
        {
            ParseSchema("ArrayOfNested");
        }

        private void ParseSchema(string schemaName)
        {
            var document = GetResourceAsString(schemaName + ".edmx");
            var schema = ResponseReader.GetSchema(document);
            Assert.Equal(1, schema.EntityTypes.Count());
            Assert.Equal(schemaName, schema.EntityTypes.First().Name);
        }
    }
}
