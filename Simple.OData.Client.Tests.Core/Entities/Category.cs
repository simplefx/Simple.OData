using System.Collections.Generic;

namespace Simple.OData.Client.Tests
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
 
        public Product[] Products { get; set; }
    }

    public class CategoryWithProductList
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public List<Product> Products { get; set; }
    }
}