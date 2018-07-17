using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Simple.OData.Client.Tests
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int? CategoryID { get; set; }

        [NotMapped]
        public int NotMappedProperty { get; set; }
        [Column(Name = "EnglishName")]
        public string MappedEnglishName { get; set; }

        public Category Category { get; set; }

        public Product()
        {
            this.NotMappedProperty = 42;
        }
    }

    public class ExtendedProduct : Product
    {
    }

    public class ProductWithUnmappedProperty : Product
    {
        public string UnmappedName { get; set; }
    }

    public class ProductWithNoCategoryLink
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int? CategoryID { get; set; }
    }

    [Table("Product")]
    public class ProductWithRemappedColumn
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int? CategoryID { get; set; }

        [NotMapped]
        public int EnglishName { get; set; }
        [Column(Name = "EnglishName")]
        public string MappedEnglishName { get; set; }

        public Category Category { get; set; }

        public ProductWithRemappedColumn()
        {
            this.EnglishName = 42;
        }
    }
}
