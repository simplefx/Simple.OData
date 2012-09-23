using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTests
{
    using Xunit;

    public class FindAssociationTest : TestBase
    {
        [Fact]
        public void FindAllProductsFromCategory()
        {
            // expected request: Products?$filter=Category/CategoryName+eq+%27Beverages%27
            IEnumerable<dynamic> products = _db.Products.FindAll(_db.Products.Category.CategoryName == "Beverages");

            Assert.NotEmpty(products);
            foreach (var product in products)
            {
                Assert.True(product.ProductID > 0);
                Assert.Equal(1, product.CategoryID);
            }
        }

        [Fact]
        public void FindAllCustomersGotoOrders()
        {
            // expected request: Customers('ALFKI')/Orders
            IEnumerable<dynamic> orders = _db.Customers.Orders.Get("ALFKI");

            Assert.NotEmpty(orders);
            foreach (var order in orders)
            {
                Assert.True(order.OrderID > 0);
                Assert.Equal("ALFKI", order.CustomerID);
            }
        }

        [Fact]
        public void FindAllEmployeeSubordinates()
        {
            // expected request: Employees(1)/Subordinates
            IEnumerable<dynamic> subordinates = _db.Employees.Subordinates.Get(1);

            Assert.NotEmpty(subordinates);
        }

        [Fact]
        public void FindAllSuperiorEmployees()
        {
            // expected request: Employees?$filter=Superior/FirstName+eq+%27Nancy%27 and Superior/LastName+eq+%27Davolio%27
            IEnumerable<dynamic> employees = _db.Employees.FindAll(_db.Employees.Superior.FirstName == "Nancy" && _db.Employees.Superior.LastName == "Davolio");

            Assert.NotEmpty(employees);
        }

        [Fact]
        public void FindEmployeeSuperior()
        {
            // expected request: Employees(1)/Superior
            var superior = _db.Employees.Superior.Get(1);

            Assert.NotNull(superior);
        }

        [Fact]
        public void FindAllEmployeeOrders()
        {
            // expected request: Orders?$filter=Employee/FirstName+eq+%27Andrew%27 and Employee/LastName+eq+%27Fuller%27
            IEnumerable<dynamic> orders = _db.Orders.FindAll(_db.Orders.Employee.FirstName == "Andrew" && _db.Orders.Employee.LastName == "Fuller");

            Assert.NotEmpty(orders);
        }

        [Fact]
        public void FindProductCategory()
        {
            // expected request: Categories?filter=Products/ProductName+eq+%27Chai%27
            var category = _db.Category.Find(_db.Category.Products.ProductName == "Chai");

            Assert.Equal("Beverages", category.CategoryName);
        }

        [Fact]
        public void FindProductSupplier()
        {
            // expected request: Suppliers?filter=Products/ProductName+eq+%27Chai%27
            var supplier = _db.Supplier.Find(_db.Supplier.Products.ProductName == "Chai");

            Assert.Equal("Exotic Liquids", supplier.CompanyName);
        }
    }
}
