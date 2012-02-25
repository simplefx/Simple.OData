using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Services.Common;

namespace Simple.Data.OData.NorthwindModel.Entities
{
    [DataServiceKey("ProductID")]
    public class Products
    {
        private int _supplierID;
        private int _categoryID;

        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int SupplierID
        {
            get { return _supplierID; }
            set { this.Supplier = NorthwindContext.Instance.SetProductSupplier(this, value); _supplierID = value; }
        }
        public int CategoryID
        {
            get { return _categoryID; }
            set { this.Category = NorthwindContext.Instance.SetProductCategory(this, value); _categoryID = value; }
        }
        public string QuantityPerUnit { get; set; }
        public decimal? UnitPrice { get; set; }
        public short UnitsInStock { get; set; }
        public short UnitsOnOrder { get; set; }
        public short ReorderLevel { get; set; }
        public bool Discontinued { get; set; }

        public Categories Category { get; set; }
        public Suppliers Supplier { get; set; }
        public ICollection<OrderDetails> OrderDetails { get; private set; }

        public Products()
        {
            this.OrderDetails = new List<OrderDetails>();
        }
    }
}
