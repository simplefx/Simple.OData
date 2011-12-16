using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.OData.IntegrationTest
{
    public class TestBase
    {
        //private const string _northwindUrl = "http://services.odata.org/Northwind/Northwind.svc/";
        private const string _northwindUrl = "http://localhost:50555/Northwind.svc/";
        protected dynamic _db;

        public TestBase()
        {
            _db = Database.Opener.Open(_northwindUrl);

            CreateTestData();
        }

        protected void CreateTestData()
        {
            _db.Customers.DeleteAll();
            _db.Employees.DeleteAll();
            _db.Products.DeleteAll();
            _db.Orders.DeleteAll();
            _db.OrderDetails.DeleteAll();

            _db.Customers.Insert(CustomerID: "ALFKI", CompanyName: "Alfreds Futterkiste");
            _db.Employees.Insert(EmployeeID: 1, FirstName: "Nancy", LastName: "Davolio");
            _db.Products.Insert(ProductID: 1, ProductName: "Chai", UnitPrice: 18m);
            _db.Products.Insert(ProductID: 2, ProductName: "Chang", UnitPrice: 19m);
            _db.Orders.Insert(OrderID: 10255, CustomerID: "ALFKI");
            _db.OrderDetails.Insert(OrderID: 10255, ProductID: 2, UnitPrice: 15m, Quantity: 20);
        }
    }
}
