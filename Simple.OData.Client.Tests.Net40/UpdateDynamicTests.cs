using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class UpdateDynamicTests : TestBase
    {
        [Fact]
        public void UpdateByKey()
        {
            var x = ODataDynamic.Expression;
            var product = _client
                .For(x.Products)
                .Set(x.ProductName = "Test1", x.UnitPrice = 18m)
                .InsertEntry();

            _client
                .For(x.Products)
                .Key(x.ProductID)
                .Set(x.UnitPrice = 123m)
                .UpdateEntry();

            product = _client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .FindEntry();

            Assert.Equal(123m, product["UnitPrice"]);
        }

        [Fact]
        public void UpdateByFilter()
        {
            var x = ODataDynamic.Expression;
            var product = _client
                .For(x.Products)
                .Set(x.ProductName = "Test1", x.UnitPrice = 18m)
                .InsertEntry();

            _client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .Set(x.UnitPrice = 123m)
                .UpdateEntry();

            product = _client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .FindEntry();

            Assert.Equal(123m, product["UnitPrice"]);
        }

        [Fact]
        public void UpdateByObjectAsKey()
        {
            var x = ODataDynamic.Expression;
            var product = _client
                .For(x.Products)
                .Set(x.ProductName = "Test1", x.UnitPrice = 18m)
                .InsertEntry();

            _client
                .For(x.Products)
                .Key(product)
                .Set(x.UnitPrice = 456m)
                .UpdateEntry();

            product = _client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .FindEntry();

            Assert.Equal(456m, product["UnitPrice"]);
        }

        [Fact]
        public void UpdateDate()
        {
            var x = ODataDynamic.Expression;
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);

            var employee = _client
                .For(x.Employees)
                .Set(x.FirstName = "Test1", x.LastName = "Test1", x.HireDate = today)
                .InsertEntry();

            _client
                .For(x.Employees)
                .Key(x.EmployeeID)
                .Set(x.HireDate = tomorrow)
                .UpdateEntry();

            employee = _client
                .For(x.Employees)
                .Key(x.EmployeeID)
                .FindEntry();

            Assert.Equal(tomorrow, employee["HireDate"]);
        }

        [Fact]
        public void AddSingleAssociation()
        {
            var x = ODataDynamic.Expression;
            var category = _client
                .For(x.Categories)
                .Set(x.CategoryName = "Test1")
                .InsertEntry();
            var product = _client
                .For(x.Products)
                .Set(x.ProductName = "Test2", x.UnitPrice = 18m)
                .InsertEntry();

            _client
                .For(x.Products)
                .Key(x.ProductID)
                .Set(x.Category = category)
                .UpdateEntry();

            product = _client
                .For(x.Products)
                .Filter(x.ProductID == product["ProductID"])
                .FindEntry();
            Assert.Equal(category["CategoryID"], product["CategoryID"]);
            category = _client
                .For(x.Categories)
                .Filter(x.CategoryID == category["CategoryID"])
                .Expand(x.Products)
                .FindEntry();
            Assert.Equal(1, (category["Products"] as IEnumerable<object>).Count());
        }

        [Fact]
        public void UpdateSingleAssociation()
        {
            var x = ODataDynamic.Expression;
            var category = _client
                .For(x.Categories)
                .Set(x.CategoryName = "Test1")
                .InsertEntry();
            var product = _client
                .For(x.Products)
                .Set(x.ProductName = "Test2", x.UnitPrice = 18m, x.CategoryID = 1)
                .InsertEntry();

            _client
                .For(x.Products)
                .Key(x.ProductID)
                .Set(x.Category = category)
                .UpdateEntry();

            product = _client
                .For(x.Products)
                .Filter(x.ProductID == product["ProductID"])
                .FindEntry();
            Assert.Equal(category["CategoryID"], product["CategoryID"]);
            category = _client
                .For(x.Categories)
                .Filter(x.CategoryID == category["CategoryID"])
                .Expand(x.Products)
                .FindEntry();
            Assert.Equal(1, (category["Products"] as IEnumerable<object>).Count());
        }

        [Fact]
        public void RemoveSingleAssociation()
        {
            var x = ODataDynamic.Expression;
            var category = _client
                .For(x.Categories)
                .Set(x.CategoryName = "Test6")
                .InsertEntry();
            var product = _client
                .For(x.Products)
                .Set(x.ProductName = "Test7", x.UnitPrice = 18m, x.Category = category)
                .InsertEntry();

            _client
                .For(x.Products)
                .Key(x.ProductID)
                .Set(x.Category = (int?)null)
                .UpdateEntry();

            product = _client
                .For(x.Products)
                .Filter(x.ProductID == product["ProductID"])
                .FindEntry();
            Assert.Null(product["CategoryID"]);
        }

        [Fact]
        public void UpdateMultipleAssociations()
        {
            var x = ODataDynamic.Expression;
            var category = _client
                .For(x.Categories)
                .Set(x.CategoryName = "Test3")
                .InsertEntry();
            var product1 = _client
                .For(x.Products)
                .Set(x.ProductName = "Test4", x.UnitPrice = 18m, x.CategoryID = 1)
                .InsertEntry();
            var product2 = _client
                .For(x.Products)
                .Set(x.ProductName = "Test5", x.UnitPrice = 18m, x.CategoryID = 1)
                .InsertEntry();

            _client
                .For(x.Categories)
                .Key(x.CategoryID)
                .Set(x.Products = new[] { product1, product2 })
                .UpdateEntry();

            category = _client
                .For(x.Categories)
                .Filter(x.CategoryID == category["CategoryID"])
                .Expand(x.Products)
                .FindEntry();
            Assert.Equal(2, (category["Products"] as IEnumerable<object>).Count());
        }
    }
}
