using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.OData.NorthwindModel;

namespace Simple.Data.OData.IntegrationTest
{
    public class TestBase : IDisposable
    {
        protected TestService _service;
        //private const string _northwindUrl = "http://services.odata.org/Northwind/Northwind.svc/";
        //private const string _northwindUrl = "http://localhost.:50555/Northwind.svc/";
        protected dynamic _db;

        public TestBase()
        {
            _service = new TestService(typeof(NorthwindService));
            _db = Database.Opener.Open(_service.ServiceUri);
            Database.SetPluralizer(new EntityPluralizer());

            CreateTestData();
        }

        public void Dispose()
        {
            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
        }

        protected void CreateTestData()
        {
            _db.Customers.DeleteAll();
            _db.Employees.DeleteAll();
            _db.Categories.DeleteAll();
            _db.Suppliers.DeleteAll();
            _db.Products.DeleteAll();
            _db.Orders.DeleteAll();
            _db.OrderDetails.DeleteAll();

            _db.Customers.Insert(CustomerID: "ALFKI", CompanyName: "Alfreds Futterkiste");
            _db.Employees.Insert(EmployeeID: 1, FirstName: "Nancy", LastName: "Davolio");
            _db.Categories.Insert(CategoryID: 1, CategoryName: "Beverages");
            _db.Suppliers.Insert(SupplierID: 1, CompanyName: "Exotic Liquids");
            _db.Products.Insert(ProductID: 1, ProductName: "Chai", UnitPrice: 18m, CategoryID: 1);
            _db.Products.Insert(ProductID: 2, ProductName: "Chang", UnitPrice: 19m, CategoryID: 1);
            _db.Orders.Insert(OrderID: 10255, CustomerID: "ALFKI");
            _db.OrderDetails.Insert(OrderID: 10255, ProductID: 2, UnitPrice: 15m, Quantity: 20);
        }
    }
}
