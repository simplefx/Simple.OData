using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests.FluentApi
{
    public class UpdateDynamicTests : TestBase
    {
        [Fact]
        public async Task UpdateByKey()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var product = await client
                .For(x.Products)
                .Set(x.ProductName = "Test1", x.UnitPrice = 18m)
                .InsertEntryAsync();

            await client
                .For(x.Products)
                .Key(product.ProductID)
                .Set(x.UnitPrice = 123m)
                .UpdateEntryAsync();

            product = await client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .FindEntryAsync();

            Assert.Equal(123m, product.UnitPrice);
        }

        [Fact]
        public async Task UpdateByFilter()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var product = await client
                .For(x.Products)
                .Set(x.ProductName = "Test1", x.UnitPrice = 18m)
                .InsertEntryAsync();

            await client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .Set(x.UnitPrice = 123m)
                .UpdateEntryAsync();

            product = await client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .FindEntryAsync();

            Assert.Equal(123m, product.UnitPrice);
        }

        [Fact]
        public async Task UpdateMultipleWithResult()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var product = await client
                .For(x.Products)
                .Set(x.ProductName = "Test1", x.UnitPrice = 18m)
                .InsertEntryAsync();

            product = (await client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .Set(x.UnitPrice = 123m)
                .UpdateEntriesAsync() as IEnumerable<dynamic>).Single();

            Assert.Equal(123m, product.UnitPrice);
        }

        [Fact]
        public async Task UpdateMultipleNoResult()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var product = await client
                .For(x.Products)
                .Set(x.ProductName = "Test1", x.UnitPrice = 18m)
                .InsertEntryAsync();

            product = (await client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .Set(x.UnitPrice = 123m)
                .UpdateEntriesAsync(false) as IEnumerable<dynamic>).Single();
            Assert.Null(product);

            product = await client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .FindEntryAsync();

            Assert.Equal(123m, product.UnitPrice);
        }

        [Fact]
        public async Task UpdateByObjectAsKey()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var product = await client
                .For(x.Products)
                .Set(x.ProductName = "Test1", x.UnitPrice = 18m)
                .InsertEntryAsync();

            await client
                .For(x.Products)
                .Key(product)
                .Set(x.UnitPrice = 456m)
                .UpdateEntryAsync();

            product = await client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .FindEntryAsync();

            Assert.Equal(456m, product.UnitPrice);
        }

        [Fact]
        public async Task UpdateDate()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var today = DateTime.Parse("2018-05-20T20:30:40.6770000");
            var tomorrow = today.AddDays(1);

            var employee = await client
                .For(x.Employees)
                .Set(x.FirstName = "Test1", x.LastName = "Test1", x.HireDate = today)
                .InsertEntryAsync();

            await client
                .For(x.Employees)
                .Key(employee.EmployeeID)
                .Set(x.HireDate = tomorrow)
                .UpdateEntryAsync();

            employee = await client
                .For(x.Employees)
                .Key(employee.EmployeeID)
                .FindEntryAsync();

            Assert.Equal(tomorrow, employee.HireDate);
        }

        [Fact]
        public async Task AddSingleAssociation()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var category = await client
                .For(x.Categories)
                .Set(x.CategoryName = "Test1")
                .InsertEntryAsync();
            var product = await client
                .For(x.Products)
                .Set(x.ProductName = "Test2", x.UnitPrice = 18m)
                .InsertEntryAsync();

            await client
                .For(x.Products)
                .Key(product.ProductID)
                .Set(x.Category = category)
                .UpdateEntryAsync();

            product = await client
                .For(x.Products)
                .Filter(x.ProductID == product.ProductID)
                .FindEntryAsync();
            Assert.Equal(category.CategoryID, product.CategoryID);
            category = await client
                .For(x.Categories)
                .Filter(x.CategoryID == category.CategoryID)
                .Expand(x.Products)
                .FindEntryAsync();
            Assert.Single((category.Products as IEnumerable<dynamic>));
        }

        [Fact]
        public async Task UpdateSingleAssociation()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var category = await client
                .For(x.Categories)
                .Set(x.CategoryName = "Test1")
                .InsertEntryAsync();
            var product = await client
                .For(x.Products)
                .Set(x.ProductName = "Test2", x.UnitPrice = 18m, x.CategoryID = 1)
                .InsertEntryAsync();

            await client
                .For(x.Products)
                .Key(product.ProductID)
                .Set(x.Category = category)
                .UpdateEntryAsync();

            product = await client
                .For(x.Products)
                .Filter(x.ProductID == product.ProductID)
                .FindEntryAsync();
            Assert.Equal(category.CategoryID, product.CategoryID);
            category = await client
                .For(x.Categories)
                .Filter(x.CategoryID == category.CategoryID)
                .Expand(x.Products)
                .FindEntryAsync();
            Assert.Single((category.Products as IEnumerable<dynamic>));
        }

        [Fact]
        public async Task RemoveSingleAssociation()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var category = await client
                .For(x.Categories)
                .Set(x.CategoryName = "Test6")
                .InsertEntryAsync();
            var product = await client
                .For(x.Products)
                .Set(x.ProductName = "Test7", x.UnitPrice = 18m, x.Category = category)
                .InsertEntryAsync();

            await client
                .For(x.Products)
                .Key(product.ProductID)
                .Set(x.Category = null)
                .UpdateEntryAsync();

            product = await client
                .For(x.Products)
                .Filter(x.ProductID == product.ProductID)
                .FindEntryAsync();
            Assert.Null(product.CategoryID);
        }

        [Fact]
        public async Task UpdateMultipleAssociations()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var category = await client
                .For(x.Categories)
                .Set(x.CategoryName = "Test3")
                .InsertEntryAsync();
            var product1 = await client
                .For(x.Products)
                .Set(x.ProductName = "Test4", x.UnitPrice = 18m, x.CategoryID = 1)
                .InsertEntryAsync();
            var product2 = await client
                .For(x.Products)
                .Set(x.ProductName = "Test5", x.UnitPrice = 18m, x.CategoryID = 1)
                .InsertEntryAsync();

            await client
                .For(x.Categories)
                .Key(category.CategoryID)
                .Set(x.Products = new[] { product1, product2 })
                .UpdateEntryAsync();

            category = await client
                .For(x.Categories)
                .Filter(x.CategoryID == category.CategoryID)
                .Expand(x.Products)
                .FindEntryAsync();
            Assert.Equal(2, (category.Products as IEnumerable<dynamic>).Count());
        }

        [Fact]
        public async Task UpdateDerived()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var ship = await client
                .For(x.Transport)
                .As(x.Ship)
                .Set(x.ShipName = "Test1")
                .InsertEntryAsync();

            ship = await client
                .For(x.Transport)
                .As(x.Ship)
                .Key(ship.TransportID)
                .Set(x.ShipName = "Test2")
                .UpdateEntryAsync();

            Assert.Equal("Test2", ship.ShipName);
        }
    }
}
