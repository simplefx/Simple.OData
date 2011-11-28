using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTest
{
    using Xunit;

    public class NortwindTest : TestBase
    {
        [Fact(Skip = "Not supported")]
        public void ShouldFindAllCategoryProducts()
        {
            var products = _db.Products.Find(
                _db.Products.Category.CategoryName == "Beverages");

            Assert.True(products.Count() > 0);
        }

        [Fact]
        public void ShouldFindCustomerByCustomerID()
        {
            var customer = _db.Customers.FindByCustomerID("ALFKI");

            Assert.Equal("ALFKI", customer.CustomerID);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldFindAllCustomerOrders()
        {
            var customerOrders = _db.Orders.Find(
                _db.Orders.Customer.CustomerID == "ALFKI");

            Assert.True(customerOrders.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldFindAllEmployeeSubordinates()
        {
            var subordinates = _db.Subordinates.Find(
                _db.Subordinates.Employees.FirstName == "Andrew" &&
                _db.Subordinates.Employees.LastName == "Fuller");

            Assert.True(subordinates.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldFindEmployeeSuperior()
        {
            var employee = _db.Employees.FindByEmployeeFirstNameAndLastName("Nancy", "Davolio");
            var superior = employee.Superior.FindOne();
            Assert.NotNull(superior);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldFindAllEmployeeOrders()
        {
            var orders = _db.Orders.Find(
                _db.Orders.Employee.FirstName == "Andrew" &&
                _db.Orders.Employee.LastName == "Fuller");

            Assert.True(orders.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldFindProductCategory()
        {
            var category = _db.Category.FindOne(
                _db.Category.Products.ProductName == "Chai");

            Assert.NotNull(category);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldFindProductSupplier()
        {
            var supplier = _db.Supplier.FindOne(
                _db.Supplier.Product.ProductName == "Chai");

            Assert.NotNull(supplier);
        }
    }
}
