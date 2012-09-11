using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Services.Common;

namespace Simple.Data.OData.NorthwindModel.Entities
{
    [DataServiceKey("ProductID")]
    public class Products
    {
        private static int _nextID;
        private Suppliers _supplier;
        private Categories _category;

        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int SupplierID { get; set; }
        public int CategoryID { get; set; }
        public string QuantityPerUnit { get; set; }
        public decimal? UnitPrice { get; set; }
        public short UnitsInStock { get; set; }
        public short UnitsOnOrder { get; set; }
        public short ReorderLevel { get; set; }
        public bool Discontinued { get; set; }

        public Suppliers Supplier
        {
            get { return _supplier; }
            set { _supplier = NorthwindContext.Instance.SetProductSupplierID(this, value.SupplierID); }
        }
        public Categories Category
        {
            get { return _category; }
            set { _category = NorthwindContext.Instance.SetProductCategoryID(this, value.CategoryID); }
        }
        public ICollection<OrderDetails> OrderDetails { get; private set; }

        public Products()
        {
            this.ProductID = ++_nextID;
            this.OrderDetails = new List<OrderDetails>();
        }
    }
}
