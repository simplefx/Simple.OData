namespace Simple.OData.Client.Tests
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
 
        public Product[] Products { get; set; }
    }
}