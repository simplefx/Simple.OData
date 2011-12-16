using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Linq;

namespace Simple.Data.OData.NorthwindModel.Entities
{
    [DataServiceKey("TerritoryID")]
    public class Territories
    {
        public string TerritoryID { get; set; }
        public string TerritoryDescription { get; set; }
        public int RegionID { get; set; }

        public ICollection<Employees> Employees { get; private set; }
        public Regions Region { get; set; }

        public Territories()
        {
            this.Employees = new List<Employees>();
        }
    }
}
