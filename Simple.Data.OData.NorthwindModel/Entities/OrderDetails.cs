using System;
using System.Data.Services.Common;
using System.Linq;

namespace Simple.Data.OData.NorthwindModel.Entities
{
    [DataServiceKey(new string[] { "OrderID", "ProductID" })]
    public class OrderDetails
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public decimal UnitPrice { get; set; }
        public short Quantity { get; set; }
        public float Discount { get; set; }

        public Orders Order { get; set; }
        public Products Product { get; set; }
    }
}
