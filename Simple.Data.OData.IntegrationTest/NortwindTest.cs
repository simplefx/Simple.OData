using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTest
{
    using Xunit;

    public class NortwindTest
    {
        private const string _northwindUrl = "http://services.odata.org/Northwind/Northwind.svc/";
        dynamic _db;

        public NortwindTest()
        {
            _db = Database.Opener.Open(_northwindUrl);
        }

        [Fact]
        public void ShouldRetrieveNonemptyCategories()
        {
            var categories = _db.Categories.All();
            Assert.True(categories.Count() > 0);
        }

        [Fact]
        public void ShouldRetrieveNonemptyCategory()
        {
            var key = "Condiments";
            var category = _db.Categories.FindByCategoryName(key);
            Assert.Equal(key, category.CategoryName);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonemptyCategoryProducts()
        {
            var key = "Condiments";
            var category = _db.Categories.FindByCategoryName(key);
            var products = category.Products.All();
            Assert.True(products.Count() > 0);
        }

        [Fact]
        public void ShouldRetrieveNonemptyCustomers()
        {
            var customers = _db.Customers.All();
            Assert.True(customers.Count() > 0);
        }

        [Fact]
        public void ShouldRetrieveNonemptyCustomer()
        {
            var key = "ALFKI";
            var customer = _db.Customers.FindByCustomerID(key);
            Assert.Equal(key, customer.CustomerID);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonemptyCustomerOrders()
        {
            var customer = _db.Customers.FindByCustomerID("ALFKI");
            var customerOrders = customer.Orders.All();
            Assert.True(customerOrders.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveEmptyCustomerCustomerDemographics()
        {
            var customer = _db.Customers.FindByCustomerID("ALFKI");
            var customerDemographics = customer.CustomerDemographics.All();
            Assert.Equal(0, customerDemographics.Count());
        }

        [Fact]
        public void ShouldRetrieveEmptyCustomerDemographics()
        {
            var customerDemographics = _db.CustomerDemographics.All();
            Assert.Equal(0, customerDemographics.Count());
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveEmptyCustomerDemographicsCustomer()
        {
            var customerDemographics = _db.CustomerDemographics.FindByCustomerID(string.Empty);
            var customers = customerDemographics.Customers.All();
            Assert.Equal(0, customers.Count());
        }

        [Fact]
        public void ShouldRetrieveNonemptyEmployes()
        {
            var employees = _db.Employees.All();
            Assert.True(employees.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonemptyEmployeeSubordinates()
        {
            var employee = _db.Employees.FindByEmployeeFirstNameAndLastName("Andrew", "Fuller");
            var subordinates = employee.Subordinates.All();
            Assert.True(subordinates.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonemptyEmployeeSuperior()
        {
            var employee = _db.Employees.FindByEmployeeFirstNameAndLastName("Nancy", "Davolio");
            var superior = employee.Superior.FindOne();
            Assert.NotNull(superior);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonemptyEmployeeOrders()
        {
            var employee = _db.Employees.FindByEmployeeFirstNameAndLastName("Andrew", "Fuller");
            var orders = employee.Orders.All();
            Assert.True(orders.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonemptyEmployeeTerritories()
        {
            var employee = _db.Employees.FindByEmployeeFirstNameAndLastName("Andrew", "Fuller");
            var territories = employee.Territories.All();
            Assert.True(territories.Count() > 0);
        }

        [Fact]
        public void ShouldRetrieveNonemptyOrders()
        {
            var orders = _db.Orders.All();
            Assert.True(orders.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonemptyOrderOrderDetails()
        {
            var order = _db.Orders.FindByOrderID(10248);
            var orderDetails = order.OrderDetails.All();
            Assert.True(orderDetails.Count() > 0);
        }

        [Fact]
        public void ShouldRetrieveNonemptyOrder()
        {
            var order = _db.Orders.FindByOrderID(10248);
            Assert.Equal(10248, order.OrderID);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonemptyOrderCustomer()
        {
            var order = _db.Orders.FindByOrderID(10248);
            var customer = order.Customer.FindOne();
            Assert.NotNull(customer);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonnullOrderEmployee()
        {
            var order = _db.Orders.FindByOrderID(10248);
            var employee = order.Employee.FindOne();
            Assert.NotNull(employee);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonnullOrderShipper()
        {
            var order = _db.Orders.FindByOrderID(10248);
            var shipper = order.Shipper.FindOne();
            Assert.NotNull(shipper);
        }

        [Fact]
        public void ShouldRetrieveNonemptyOrderDetails()
        {
            var orderDetails = _db.Order_Details.All();
            Assert.True(orderDetails.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonnullOrderDetailsOrder()
        {
            var orderDetails = _db.OrderDetails.FindByOrderIDAndOrderDetailsID(10248, 11);
            var order = orderDetails.Order.FindOne();
            Assert.NotNull(order);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonnullOrderDetailsProduct()
        {
            var orderDetails = _db.OrderDetails.FindByOrderIDAndOrderDetailsID(10248, 11);
            var product = orderDetails.Product.FindOne();
            Assert.NotNull(product);
        }

        [Fact]
        public void ShouldRetrieveNonemptyProducts()
        {
            var products = _db.Products.All();
            Assert.True(products.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonnullProductCategory()
        {
            var product = _db.Products.FindByProductName("Chai");
            var category = product.Category.FindOne();
            Assert.NotNull(category);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonnullProductSupplier()
        {
            var product = _db.Products.FindByProductName("Chai");
            var supplier = product.Supplier.FindOne();
            Assert.NotNull(supplier);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonemptyProductOrderDetails()
        {
            var product = _db.Products.FindByProductName("Chai");
            var orderDetails = product.OrderDetails.All();
            Assert.True(orderDetails.Count() > 0);
        }

        [Fact]
        public void ShouldRetrieveNonemptyRegions()
        {
            var regions = _db.Regions.All();
            Assert.True(regions.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonemptyRegionTerritories()
        {
            var region = _db.Regions.FindByRegionName("Eastern");
            var territories = region.Territories.All();
            Assert.True(territories.Count() > 0);
        }

        [Fact]
        public void ShouldRetrieveNonemptyShippers()
        {
            var shippers = _db.Shippers.All();
            Assert.True(shippers.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonemptyShipperOrders()
        {
            var shipper = _db.Shippers.FindOne("Speedy Express");
            var orders = shipper.Orders.All();
            Assert.True(orders.Count() > 0);
        }

        [Fact]
        public void ShouldRetrieveNonemptySuppliers()
        {
            var suppliers = _db.Suppliers.All();
            Assert.True(suppliers.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonemptySupplierProducts()
        {
            var supplier = _db.Suppliers.FindBySupplierName("Exotic Liquids");
            var products = supplier.Products.All();
            Assert.True(products.Count() > 0);
        }

        [Fact]
        public void ShouldRetrieveNonemptyTerritories()
        {
            var territories = _db.Territories.All();
            Assert.True(territories.Count() > 0);
        }

        [Fact(Skip = "Not supported")]
        public void ShouldRetrieveNonnullTerritoryRegion()
        {
            var territory = _db.Territory.FindByTerritoryID("Westboro");
            var region = territory.Region.FindOne();
            Assert.NotNull(region);
        }
    }
}
