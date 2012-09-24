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
        public void FindAllEmployeesFromSuperior()
        {
            // expected request: Employees?$filter=Superior/FirstName+eq+%27Nancy%27 and Superior/LastName+eq+%27Davolio%27
            IEnumerable<dynamic> employees = _db.Employees.FindAll(_db.Employees.Superior.FirstName == "Andrew" && _db.Employees.Superior.LastName == "Fuller");

            Assert.NotEmpty(employees);
            foreach (var employee in employees)
            {
                Assert.True(employee.EmployeeID > 0);
                Assert.Equal(2, employee.ReportsTo);
            }
        }

        [Fact]
        public void FindAllOrdersFromEmployee()
        {
            // expected request: Orders?$filter=Employee/FirstName+eq+%27Andrew%27 and Employee/LastName+eq+%27Fuller%27
            IEnumerable<dynamic> orders = _db.Orders.FindAll(_db.Orders.Employee.FirstName == "Andrew" && _db.Orders.Employee.LastName == "Fuller");

            Assert.NotEmpty(orders);
            foreach (var order in orders)
            {
                Assert.Equal(2, order.EmployeeID);
            }
        }

        [Fact]
        public void FindAllCustomerOrders()
        {
            // expected request: Customers('ALFKI')/Orders
            IEnumerable<dynamic> orders = _db.Customers.Orders.FindAll(_db.Customers.CustomerID == "ALFKI");

            Assert.NotEmpty(orders);
            foreach (var order in orders)
            {
                Assert.True(order.OrderID > 0);
                Assert.Equal("ALFKI", order.CustomerID);
            }
        }

        [Fact]
        public void FindAllOrderOrderDetails()
        {
            // expected request: Orders(10952)/OrderDetails
            IEnumerable<dynamic> orderDetails = _db.Orders.OrderDetails.FindAll(_db.Orders.OrderID == 10952);

            Assert.NotEmpty(orderDetails);
            foreach (var details in orderDetails)
            {
                Assert.True(details.ProductID > 0);
                Assert.Equal(10952, details.OrderID);
            }
        }

        [Fact]
        public void FindAllEmployeeSubordinates()
        {
            // expected request: Employees(1)/Subordinates
            IEnumerable<dynamic> subordinates = _db.Employees.Subordinates.FindAll(_db.Employees.EmployeeID == 2);

            Assert.NotEmpty(subordinates);
            foreach (var subordinate in subordinates)
            {
                Assert.True(subordinate.EmployeeID > 0);
                Assert.Equal(2, subordinate.ReportsTo);
            }
        }

        [Fact]
        public void GetEmployeeSuperior()
        {
            // expected request: Employees(1)/Superior
            var superior = _db.Employees.Superior.Get(1);

            Assert.NotNull(superior);
            Assert.Equal(2, superior.EmployeeID);
        }

        [Fact]
        public void FindEmployeeSuperior()
        {
            // expected request: Employees(1)/Superior
            var superior = _db.Employees.Superior.Find(_db.Employees.EmployeeID == 1);

            Assert.NotNull(superior);
            Assert.Equal(2, superior.EmployeeID);
        }

        [Fact]
        public void FindAllEmployeeSuperiors()
        {
            // expected request: Employees(1)/Superior
            IEnumerable<dynamic> superiors = _db.Employees.Superior.FindAll(_db.Employees.EmployeeID == 1);

            Assert.NotEmpty(superiors);
            Assert.Equal(1, superiors.Count());
            Assert.Equal(2, superiors.First().EmployeeID);
        }
    }
}
