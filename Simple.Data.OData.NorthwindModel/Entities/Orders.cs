using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Linq;

namespace Simple.Data.OData.NorthwindModel.Entities
{
    [DataServiceKey("OrderID")]
    public class Orders
    {
        public int OrderID { get; set; }
        public string CustomerID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public int ShipVia { get; set; }
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
            this.OrderDetails = new List<OrderDetails>();
        }
    }
}
