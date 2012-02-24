using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Simple.OData.Test
{
    public class DataServicesHelperTests
    {
        private const int productProperties = 10;
        private const int categoryProperties = 4;

        [Fact]
        public void GetDataParsesSingleProduct()
        {
            string document = GetResourceAsString("SingleProduct.xml");
            var result = DataServicesHelper.GetData(document);
            Assert.Equal(1, result.Count());
            Assert.Equal(productProperties, result.First().Count);
        }

        [Fact]
        public void GetDataParsesMultipleProducts()
        {
            string document = GetResourceAsString("MultipleProducts.xml");
            var result = DataServicesHelper.GetData(document);
            Assert.Equal(20, result.Count());
            Assert.Equal(productProperties, result.First().Count);
        }

        [Fact]
        public void GetDataParsesSingleProductWithCategory()
        {
            string document = GetResourceAsString("SingleProductWithCategory.xml");
            var result = DataServicesHelper.GetData(document);
            Assert.Equal(1, result.Count());
            Assert.Equal(productProperties + 1, result.First().Count);
            Assert.Equal(categoryProperties, (result.First()["Category"] as IDictionary<string,object>).Count);
        }

        [Fact]
        public void GetDataParsesMultipleProductsWithCategory()
        {
            string document = GetResourceAsString("MultipleProductsWithCategory.xml");
            var result = DataServicesHelper.GetData(document);
            Assert.Equal(20, result.Count());
            Assert.Equal(productProperties + 1, result.First().Count);
            Assert.Equal(categoryProperties, (result.First()["Category"] as IDictionary<string, object>).Count);
        }

        [Fact]
        public void GetDataParsesSingleCategoryWithProducts()
        {
            string document = GetResourceAsString("SingleCategoryWithProducts.xml");
            var result = DataServicesHelper.GetData(document);
            Assert.Equal(1, result.Count());
            Assert.Equal(categoryProperties + 1, result.First().Count);
            Assert.Equal(12, (result.First()["Products"] as IEnumerable<IDictionary<string, object>>).Count());
            Assert.Equal(productProperties, (result.First()["Products"] as IEnumerable<IDictionary<string, object>>).First().Count);
        }

        [Fact]
        public void GetDataParsesMultipleCategoriesWithProducts()
        {
            string document = GetResourceAsString("MultipleCategoriesWithProducts.xml");
            var result = DataServicesHelper.GetData(document);
            Assert.Equal(8, result.Count());
            Assert.Equal(categoryProperties + 1, result.First().Count);
            Assert.Equal(12, (result.First()["Products"] as IEnumerable<IDictionary<string, object>>).Count());
            Assert.Equal(productProperties, (result.First()["Products"] as IEnumerable<IDictionary<string, object>>).First().Count);
        }

        private string GetResourceAsString(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            string completeResourceName = resourceNames.FirstOrDefault(o => o.EndsWith(resourceName, StringComparison.CurrentCultureIgnoreCase));
            using (Stream resourceStream = assembly.GetManifestResourceStream(completeResourceName))
            {
                TextReader reader = new StreamReader(resourceStream);
                return reader.ReadToEnd();
            }
        }
    }
}
