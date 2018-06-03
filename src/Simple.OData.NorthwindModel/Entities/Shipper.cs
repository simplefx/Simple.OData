using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Simple.OData.NorthwindModel.Entities
{
    public class Shipper
    {
        [Key]
        public int ShipperID { get; set; }
        [Required]
        public string CompanyName { get; set; }
        public string Phone { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}