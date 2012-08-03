using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Services.Common;

namespace Simple.Data.OData.NorthwindModel.Entities
{
    [DataServiceKey("CategoryID")]
    public class Categories
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public byte[] Picture { get; set; }

        public ICollection<Products> Products { get; private set; }

        public Categories()
        {
            this.Products = new List<Products>();
        }
    }
}
