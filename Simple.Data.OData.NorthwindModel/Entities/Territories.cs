using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Linq;

namespace Simple.Data.OData.NorthwindModel.Entities
{
    [DataServiceKey("TerritoryID")]
    public class Territories
    {
        private int _regionID;

        public string TerritoryID { get; set; }
        public string TerritoryDescription { get; set; }
        public int RegionID
        {
            get { return _regionID; }
            set { this.Region = NorthwindContext.Instance.SetTerritoryRegionID(this, value); _regionID = value; }
        }

        public ICollection<Employees> Employees { get; private set; }
        public Regions Region { get; set; }

        public Territories()
        {
            this.Employees = new List<Employees>();
        }
    }
}
