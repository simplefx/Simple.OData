using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTests
{
    using Xunit;

    public class FindExpandTest : TestBase
    {
        [Fact]
        public void FindCategoryByNameExpandProducts()
        {
            // expected request: Categories?expand=Products&$filter=CategoryName+eq+'Beverages'
            var category = _db.Category.WithProducts().FindByCategoryName("Beverages");

            Assert.Equal("Beverages", category.CategoryName);
            Assert.True(category.Products.Count > 0);
        }

        [Fact]
        public void FindAllCategoriesWithProducts()
        {
            // expected request: Categories?expand=Products
            var categories = _db.Category.All().WithProducts().ToList();

            Assert.True(categories.Count > 0);
            Assert.True(categories[0].Products.Count > 0);
        }

        [Fact]
        public void FindAllProductsWithCategory()
        {
            // expected request: Products?expand=Category
            var products = _db.Products.All().WithCategory().ToList();

            Assert.True(products.Count > 0);
            Assert.Equal("Beverages", products[0].Category.CategoryName);
        }

        [Fact]
        public void GetCustomerExpandOrders()
        {
            // expected request: Customers('ALFKI')?$expand=Orders
            var customer = _db.Customer.WithOrders().Get("ALFKI");

            Assert.Equal("ALFKI", customer.CustomerID);
            Assert.True(customer.Orders.Count > 0);
        }

        [Fact]
        public void FindAllEmployeesExpandSubordinates()
        {
            // expected request: Employees?$expand=Subordinates
            var employees = _db.Employees.All().WithSubordinates().ToList();

            Assert.True(employees.Count > 0);
            Assert.True(employees[0].Subordinates.Count > 0);
        }

        [Fact]
        public void FindEmployeeWithSuperior()
        {
            // expected request: Employees?$expand=Superior&$filter=FirstName+eq+%27Nancy%27 and LastName+eq+%27Davolio%27
            var employee = _db.Employees.WithSuperior().FindByFirstNameAndLastName("Nancy", "Davolio");
            Assert.NotNull(employee);
            Assert.NotNull(employee.Superior);
        }
    }
}
