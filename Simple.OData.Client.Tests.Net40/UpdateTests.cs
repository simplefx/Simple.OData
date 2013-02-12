using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class UpdateTests : TestBase
    {
        [Fact]
        public void UpdateByKey()
        {
            var product = _client
                .For("Products")
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            _client
                .For("Products")
                .Key(product["ProductID"])
                .Set(new {UnitPrice = 123m})
                .UpdateEntry();

            product = _client
                .For("Products")
                .Filter("ProductName eq 'Test1'")
                .FindEntry();

            Assert.Equal(123m, product["UnitPrice"]);
        }

        [Fact]
        public void UpdateByFilter()
        {
            var product = _client
                .For("Products")
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            _client
                .For("Products")
                .Filter("ProductName eq 'Test1'")
                .Set(new { UnitPrice = 123m })
                .UpdateEntry();

            product = _client
                .For("Products")
                .Filter("ProductName eq 'Test1'")
                .FindEntry();

            Assert.Equal(123m, product["UnitPrice"]);
        }

        [Fact]
        public void UpdateByObjectAsKey()
        {
            var product = _client
                .For("Products")
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            _client
                .For("Products")
                .Key(product)
                .Set(new { UnitPrice = 456m })
                .UpdateEntry();

            product = _client
                .For("Products")
                .Filter("ProductName eq 'Test1'")
                .FindEntry();

            Assert.Equal(456m, product["UnitPrice"]);
        }

        [Fact]
        public void AddSingleAssociation()
        {
            var category = _client
                .For("Categories")
                .Set(new { CategoryName = "Test1" })
                .InsertEntry();
            var product = _client
                .For("Products")
                .Set(new { ProductName = "Test2", UnitPrice = 18m })
                .InsertEntry();

            _client
                .For("Products")
                .Key(product["ProductID"])
                .Set(new { Category = category })
                .UpdateEntry();

            product = _client
                .For("Products")
                .Filter("ProductID eq "+ product["ProductID"])
                .FindEntry();
            Assert.Equal(category["CategoryID"], product["CategoryID"]);
            category = _client
                .For("Categories")
                .Filter("CategoryID eq " + category["CategoryID"])
                .Expand("Products")
                .FindEntry();
            Assert.Equal(1, (category["Products"] as IEnumerable<object>).Count());
        }

        [Fact]
        public void UpdateSingleAssociation()
        {
            var category = _client
                .For("Categories")
                .Set(new { CategoryName = "Test1" })
                .InsertEntry();
            var product = _client
                .For("Products")
                .Set(new { ProductName = "Test2", UnitPrice = 18m, CategoryID = 1 })
                .InsertEntry();

            _client
                .For("Products")
                .Key(product["ProductID"])
                .Set(new { Category = category })
                .UpdateEntry();

            product = _client
                .For("Products")
                .Filter("ProductID eq " + product["ProductID"])
                .FindEntry();
            Assert.Equal(category["CategoryID"], product["CategoryID"]);
            category = _client
                .For("Categories")
                .Filter("CategoryID eq " + category["CategoryID"])
                .Expand("Products")
                .FindEntry();
            Assert.Equal(1, (category["Products"] as IEnumerable<object>).Count());
        }

        [Fact]
        public void RemoveSingleAssociation()
        {
            var category = _client
                .For("Categories")
                .Set(new { CategoryName = "Test6" })
                .InsertEntry();
            var product = _client
                .For("Products")
                .Set(new { ProductName = "Test7", UnitPrice = 18m, Category = category })
                .InsertEntry();

            _client
                .For("Products")
                .Key(product["ProductID"])
                .Set(new { Category = (int?)null })
                .UpdateEntry();

            product = _client
                .For("Products")
                .Filter("ProductID eq " + product["ProductID"])
                .FindEntry();
            Assert.Null(product["CategoryID"]);
        }

        [Fact]
        public void UpdateMultipleAssociations()
        {
            var category = _client
                .For("Categories")
                .Set(new { CategoryName = "Test3" })
                .InsertEntry();
            var product1 = _client
                .For("Products")
                .Set(new { ProductName = "Test4", UnitPrice = 18m, CategoryID = 1 })
                .InsertEntry();
            var product2 = _client
                .For("Products")
                .Set(new { ProductName = "Test5", UnitPrice = 18m, CategoryID = 1 })
                .InsertEntry();

            _client
                .For("Categories")
                .Key(category["CategoryID"])
                .Set(new { Products = new[] { product1, product2 } })
                .UpdateEntry();

            category = _client
                .For("Categories")
                .Filter("CategoryID eq " + category["CategoryID"])
                .Expand("Products")
                .FindEntry();
            Assert.Equal(2, (category["Products"] as IEnumerable<object>).Count());
        }
    }
}
