using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTest
{
    using Xunit;

    public class FindAssociationTest : TestBase
    {
        [Fact(Skip = "Associations are not implemented")]
        public void FindAllCategoryProducts()
        {
            // expected request: Products?$filter=Category/CategoryName+eq+%27Beverages%27
            var products = _db.Products.Find(
                _db.Products.Category.CategoryName == "Beverages");

            Assert.True(products.Count() > 0);
        }

        [Fact(Skip = "Associations are not implemented")]
        public void FindAllCustomerOrders()
        {
            // expected request: Customers('ALFKI')/Orders
            var customerOrders = _db.Orders.Find(
                _db.Orders.Customer.CustomerID == "ALFKI");

            Assert.True(customerOrders.Count() > 0);
        }

        [Fact(Skip = "Associations are not implemented")]
        public void FindAllEmployeeSubordinates()
        {
            // expected request: Employees(1)/Subordinates
            var subordinates = _db.Employees.Find(
                _db.Employees.Subordinates.FirstName == "Andrew" &&
                _db.Employees.Subordinates.LastName == "Fuller");

            Assert.True(subordinates.Count() > 0);
        }

        [Fact(Skip = "Associations are not implemented")]
        public void FindAllEmployeeSubordinates2()
        {
            // expected request: Employees?$filter=Superior/FirstName+eq+%27Nancy%27 and Superior/LastName+eq+%27Davolio%27
            var subordinates = _db.Employees.Find(
                _db.Employees.Subordinates.FirstName == "Andrew" &&
                _db.Employees.Subordinates.LastName == "Fuller");

            Assert.True(subordinates.Count() > 0);
        }

        [Fact(Skip = "Associations are not implemented")]
        public void FindEmployeeSuperior()
        {
            // expected request: Employees(1)/Superior
            var employee = _db.Employees.FindByFirstNameAndLastName("Nancy", "Davolio");
            var superior = employee.Superior.FindOne();
            Assert.NotNull(superior);
        }

        [Fact(Skip = "Associations are not implemented")]
        public void FindEmployeeSuperior2()
        {
            // expected request: Employees?$filter=Superior/FirstName+eq+%27Nancy%27 and Superior/LastName+eq+%27Davolio%27
            var employee = _db.Employees.FindByFirstNameAndLastName("Nancy", "Davolio");
            var superior = employee.Superior.FindOne();
            Assert.NotNull(superior);
        }

        [Fact(Skip = "Associations are not implemented")]
        public void FindAllEmployeeOrders()
        {
            // expected request: Orders?$filter=Employee/FirstName+eq+%27Andrew%27 and Employee/LastName+eq+%27Fuller%27
            var orders = _db.Orders.Find(
                _db.Orders.Employee.FirstName == "Andrew" &&
                _db.Orders.Employee.LastName == "Fuller");

            Assert.True(orders.Count() > 0);
        }

        [Fact(Skip = "Associations are not implemented")]
        public void FindProductCategory()
        {
            // expected request: Categories?filter=Products/ProductName+eq+%27Chai%27
            var category = _db.Category.FindOne(
                _db.Category.Products.ProductName == "Chai");

            Assert.Equal("Beverages", category.CategoryName);
        }

        [Fact(Skip = "Associations are not implemented")]
        public void FindProductSupplier()
        {
            // expected request: Suppliers?filter=Products/ProductName+eq+%27Chai%27
            var supplier = _db.Supplier.FindOne(
                _db.Supplier.Product.ProductName == "Chai");

            Assert.Equal("Exotic Liquids", supplier.CompanyName);
        }
    }
}
