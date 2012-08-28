using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using System.Linq;

namespace Simple.Data.OData.NorthwindModel.Entities
{
    [DataServiceKey("RegionID")]
    public class Regions
    {
        private static int _nextID;

        public int RegionID { get; set; }
        public string RegionDescription { get; set; }

        public ICollection<Territories> Territories { get; private set; }

        public Regions()
        {
            this.RegionID = ++_nextID;
            this.Territories = new List<Territories>();
        }
    }
}
