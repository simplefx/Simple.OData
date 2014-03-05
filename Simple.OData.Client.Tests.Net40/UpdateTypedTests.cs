using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class UpdateTypedTests : TestBase
    {
        [Fact]
        public void UpdateByKey()
        {
            var product = _client
                .For<Product>()
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            _client
                .For<Product>()
                .Key(product.ProductID)
                .Set(new { UnitPrice = 123m })
                .UpdateEntry();

            product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .FindEntry();

            Assert.Equal(123m, product.UnitPrice);
        }

        [Fact]
        public void UpdateByFilter()
        {
            var product = _client
                .For<Product>()
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            _client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .Set(new { UnitPrice = 123m })
                .UpdateEntry();

            product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .FindEntry();

            Assert.Equal(123m, product.UnitPrice);
        }

        [Fact]
        public void UpdateMultipleWithResult()
        {
            var product = _client
                .For<Product>()
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .Set(new { UnitPrice = 123m })
                .UpdateEntries().Single();

            Assert.Equal(123m, product.UnitPrice);
        }

        [Fact]
        public void UpdateMultipleNoResult()
        {
            var product = _client
                .For<Product>()
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .Set(new { UnitPrice = 123m })
                .UpdateEntries(false).Single();
            Assert.Null(product);

            product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .FindEntry();

            Assert.Equal(123m, product.UnitPrice);
        }

        [Fact]
        public void UpdateByObjectAsKey()
        {
            var product = _client
                .For<Product>()
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            _client
                .For<Product>()
                .Key(product)
                .Set(new { UnitPrice = 456m })
                .UpdateEntry();

            product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .FindEntry();

            Assert.Equal(456m, product.UnitPrice);
        }

        [Fact]
        public void UpdateObjectValue()
        {
            var product = _client
                .For<Product>()
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntry();

            product.UnitPrice = 456m;
            _client
                .For<Product>()
                .Key(product)
                .Set(product)
                .UpdateEntry();

            product = _client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .FindEntry();

            Assert.Equal(456m, product.UnitPrice);
        }

        [Fact]
        public void UpdateDate()
        {
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);

            var employee = _client
                .For<Employee>()
                .Set(new { FirstName = "Test1", LastName = "Test1", HireDate = today })
                .InsertEntry();

            _client
                .For<Employee>()
                .Key(employee.EmployeeID)
                .Set(new { HireDate = tomorrow })
                .UpdateEntry();

            employee = _client
                .For<Employee>()
                .Key(employee.EmployeeID)
                .FindEntry();

            Assert.Equal(tomorrow, employee.HireDate);
        }

        [Fact]
        public void AddSingleAssociation()
        {
            var category = _client
                .For<Category>()
                .Set(new { CategoryName = "Test1" })
                .InsertEntry();
            var product = _client
                .For<Product>()
                .Set(new { ProductName = "Test2", UnitPrice = 18m })
                .InsertEntry();

            _client
                .For<Product>()
                .Key(product.ProductID)
                .Set(new { Category = category })
                .UpdateEntry();

            product = _client
                .For<Product>()
                .Filter(x => x.ProductID == product.ProductID)
                .FindEntry();
            Assert.Equal(category.CategoryID, product.CategoryID);
            category = _client
                .For<Category>()
                .Filter(x => x.CategoryID == category.CategoryID)
                .Expand(x => x.Products)
                .FindEntry();
            Assert.Equal(1, category.Products.Count());
        }

        [Fact]
        public void UpdateSingleAssociation()
        {
            var category = _client
                .For<Category>()
                .Set(new { CategoryName = "Test1" })
                .InsertEntry();
            var product = _client
                .For<Product>()
                .Set(new { ProductName = "Test2", UnitPrice = 18m, CategoryID = 1 })
                .InsertEntry();

            _client
                .For<Product>()
                .Key(product.ProductID)
                .Set(new { Category = category })
                .UpdateEntry();

            product = _client
                .For<Product>()
                .Filter(x => x.ProductID == product.ProductID)
                .FindEntry();
            Assert.Equal(category.CategoryID, product.CategoryID);
            category = _client
                .For<Category>()
                .Filter(x => x.CategoryID == category.CategoryID)
                .Expand(x => x.Products)
                .FindEntry();
            Assert.Equal(1, category.Products.Count());
        }

        [Fact]
        public void RemoveSingleAssociation()
        {
            var category = _client
                .For<Category>()
                .Set(new { CategoryName = "Test6" })
                .InsertEntry();
            var product = _client
                .For<Product>()
                .Set(new { ProductName = "Test7", UnitPrice = 18m, Category = category })
                .InsertEntry();

            _client
                .For<Product>()
                .Key(product.ProductID)
                .Set(new { Category = (int?)null })
                .UpdateEntry();

            product = _client
                .For<Product>()
                .Filter(x => x.ProductID == product.ProductID)
                .FindEntry();
            Assert.Null(product.CategoryID);
        }

        [Fact]
        public void UpdateMultipleAssociations()
        {
            var category = _client
                .For<Category>()
                .Set(new { CategoryName = "Test3" })
                .InsertEntry();
            var product1 = _client
                .For<Product>()
                .Set(new { ProductName = "Test4", UnitPrice = 18m, CategoryID = 1 })
                .InsertEntry();
            var product2 = _client
                .For<Product>()
                .Set(new { ProductName = "Test5", UnitPrice = 18m, CategoryID = 1 })
                .InsertEntry();

            _client
                .For<Category>()
                .Key(category.CategoryID)
                .Set(new { Products = new[] { product1, product2 } })
                .UpdateEntry();

            category = _client
                .For<Category>()
                .Filter(x => x.CategoryID == category.CategoryID)
                .Expand(x => x.Products)
                .FindEntry();
            Assert.Equal(2, category.Products.Count());
        }
    }
}
