using System;
using System.Data.Services.Common;
using System.Linq;

namespace Simple.Data.OData.NorthwindModel.Entities
{
    [DataServiceKey(new string[] { "OrderID", "ProductID" })]
    public class OrderDetails
    {
        private int _orderID;
        private int _productID;

        public int OrderID
        {
            get { return _orderID; }
            set { this.Order = NorthwindContext.Instance.SetOrderDetailsOrder(this, value); _orderID = value; }
        }
        public int ProductID
        {
            get { return _productID; }
            set { this.Product = NorthwindContext.Instance.SetOrderDetailsProduct(this, value); _productID = value; }
        }
        public decimal UnitPrice { get; set; }
        public short Quantity { get; set; }
        public float Discount { get; set; }

        public Orders Order { get; set; }
        public Products Product { get; set; }
    }
}
