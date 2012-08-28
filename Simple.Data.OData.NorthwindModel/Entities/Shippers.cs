using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Linq;

namespace Simple.Data.OData.NorthwindModel.Entities
{
    [DataServiceKey("ShipperID")]
    public class Shippers
    {
        private static int _nextID;

        public int ShipperID { get; set; }
        public string CompanyName { get; set; }
        public string Phone { get; set; }

        public ICollection<Orders> Orders { get; private set; }

        public Shippers()
        {
            this.ShipperID = ++_nextID;
            this.Orders = new List<Orders>();
        }
    }
}
