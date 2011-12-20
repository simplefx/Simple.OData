using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTest
{
    using Xunit;

    public class FindAssociationTest : TestBase
    {
        [Fact]
        public void FindAllCategoryProducts()
        {
            var products = _db.Products.Find(
                _db.Products.Category.CategoryName == "Beverages");

            Assert.True(products.Count() > 0);
        }

        [Fact]
        public void FindAllCustomerOrders()
        {
            var customerOrders = _db.Orders.Find(
                _db.Orders.Customer.CustomerID == "ALFKI");

            Assert.True(customerOrders.Count() > 0);
        }

        [Fact]
        public void FindAllEmployeeSubordinates()
        {
            var subordinates = _db.Subordinates.Find(
                _db.Subordinates.Employees.FirstName == "Andrew" &&
                _db.Subordinates.Employees.LastName == "Fuller");

            Assert.True(subordinates.Count() > 0);
        }

        [Fact]
        public void FindEmployeeSuperior()
        {
            var employee = _db.Employees.FindByEmployeeFirstNameAndLastName("Nancy", "Davolio");
            var superior = employee.Superior.FindOne();
            Assert.NotNull(superior);
        }

        [Fact]
        public void FindAllEmployeeOrders()
        {
            var orders = _db.Orders.Find(
                _db.Orders.Employee.FirstName == "Andrew" &&
                _db.Orders.Employee.LastName == "Fuller");

            Assert.True(orders.Count() > 0);
        }

        [Fact]
        public void FindProductCategory()
        {
            var category = _db.Category.FindOne(
                _db.Category.Products.ProductName == "Chai");

            Assert.Equal("Beverages", category.CategoryName);
        }

        [Fact]
        public void FindProductSupplier()
        {
            var supplier = _db.Supplier.FindOne(
                _db.Supplier.Product.ProductName == "Chai");

            Assert.Equal("Exotic Liquids", supplier.CompanyName);
        }
    }
}
