using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTest
{
    using Xunit;

    public class NortwindTest
    {
        dynamic _db;
        private const string _northwindUrl = "http://services.odata.org/Northwind/Northwind.svc/";
        private const string _testCategoryName = "Beverages";
        private const int _testCategoryID = 1;
        private const string _testCustomerID = "ALFKI";
        private const string _testEmployeeFirstName = "Andrew";
        private const string _testEmployeeLastName = "Fuller";
        private const int _testOrderID = 10248;
        private const int _testProductID = 11;

        public NortwindTest()
        {
            _db = Database.Opener.Open(_northwindUrl);
        }

        [Fact]
        public void ShouldFindAllCategories()
        {
            var categories = _db.Categories.All();

            Assert.True(categories.Count() > 0);
        }

        [Fact]
        public void ShouldFindCategoryByCategoryName()
        {
            var category = _db.Categories.FindByCategoryName(_testCategoryName);

            Assert.Equal(_testCategoryName, category.CategoryName);
        }

        [Fact]
        public void ShouldGetCategoryByKey()
        {
            var category = _db.Categories.Get(_testCategoryID);

            Assert.Equal(_testCategoryName, category.CategoryName);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldFindAllCategoryProducts()
        {
            var products = _db.Products.Find(
                _db.Products.Category.CategoryName == _testCategoryName);

            Assert.True(products.Count() > 0);
        }

        [Fact]
        public void ShouldFindAllCustomers()
        {
            var customers = _db.Customers.All();

            Assert.True(customers.Count() > 0);
        }

        [Fact]
        public void ShouldFindCustomerByCustomerID()
        {
            var customer = _db.Customers.FindByCustomerID(_testCustomerID);

            Assert.Equal(_testCustomerID, customer.CustomerID);
        }

        [Fact]
        public void ShouldGetCustomerByKey()
        {
            var customer = _db.Customers.Get(_testCustomerID);

            Assert.Equal(_testCustomerID, customer.CustomerID);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldFindAllCustomerOrders()
        {
            var customerOrders = _db.Orders.Find(
                _db.Orders.Customer.CustomerID == _testCustomerID);

            Assert.True(customerOrders.Count() > 0);
        }

        [Fact]
        public void ShouldFindAllEmployes()
        {
            var employees = _db.Employees.All();

            Assert.True(employees.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldFindAllEmployeeSubordinates()
        {
            var subordinates = _db.Subordinates.Find(
                _db.Subordinates.Employees.FirstName == _testEmployeeFirstName && 
                _db.Subordinates.Employees.LastName == _testEmployeeLastName);

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
                _db.Orders.Employee.FirstName == _testEmployeeFirstName && 
                _db.Orders.Employee.LastName == _testEmployeeLastName);

            Assert.True(orders.Count() > 0);
        }

        [Fact]
        public void ShouldFindAllOrders()
        {
            var orders = _db.Orders.All();

            Assert.True(orders.Count() > 0);
        }

        [Fact]
        public void ShouldFindAllProducts()
        {
            var products = _db.Products.All();

            Assert.True(products.Count() > 0);
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

        [Fact(Skip = "Segments with multiple key values must specify them in 'name=value' form")]
        public void ShouldGetProductDetailsByCompoundKey()
        {
            var orderDetails = _db.Order_Details.Get(_testOrderID, _testProductID);

            Assert.Equal(_testOrderID, orderDetails.OrderID);
        }
    }
}
