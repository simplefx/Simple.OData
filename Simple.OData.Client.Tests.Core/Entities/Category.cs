using System.Collections.Generic;

namespace Simple.OData.Client.Tests
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public byte[] Picture { get; set; }
 
        public Product[] Products { get; set; }
    }

    public class CategoryWithList
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public List<Product> Products { get; set; }
    }

    public class CategoryWithIList
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public IList<Product> Products { get; set; }
    }

    public class CategoryWithICollection
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public ICollection<Product> Products { get; set; }
    }

    public class CategoryWithHashSet
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public HashSet<Product> Products { get; set; }
    }
}