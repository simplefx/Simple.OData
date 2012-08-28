using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Linq;

namespace Simple.Data.OData.NorthwindModel.Entities
{
    [DataServiceKey("OrderID")]
    public class Orders
    {
        private static int _nextID;
        private string _customerID;
        private int _employeeID;
        private int _shipVia;

        public int OrderID { get; set; }
        public string CustomerID
        {
            get { return _customerID; }
            set { this.Customer = NorthwindContext.Instance.SetOrderCustomer(this, value); _customerID = value; }
        }
        public int EmployeeID
        {
            get { return _employeeID; }
            set { this.Employee = NorthwindContext.Instance.SetOrderEmployee(this, value); _employeeID = value; }
        }
        public DateTime? OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public int ShipVia
        {
            get { return _shipVia; }
            set { this.Shipper = NorthwindContext.Instance.SetOrderShipper(this, value); _shipVia = value; }
        }
        public decimal? Freight { get; set; }
        public string ShipName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipRegion { get; set; }
        public string ShipPostalCode { get; set; }
        public string ShipCountry { get; set; }

        public ICollection<OrderDetails> OrderDetails { get; private set; }
        public Customers Customer { get; set; }
        public Employees Employee { get; set; }
        public Shippers Shipper { get; set; }

        public Orders()
        {
            this.OrderID = ++_nextID;
            this.OrderDetails = new List<OrderDetails>();
        }
    }
}
