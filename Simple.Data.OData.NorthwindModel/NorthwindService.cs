using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.Text;
using Simple.Data.OData.NorthwindModel;

namespace Simple.Data.OData.NorthwindModel
{
    public class NorthwindService : DataService<NorthwindContext>
    {
        public static void InitializeService(DataServiceConfiguration config)
        {
            config.SetEntitySetAccessRule("*", EntitySetRights.All);
            config.SetServiceOperationAccessRule("*", ServiceOperationRights.All);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3;
            config.UseVerboseErrors = true;
        }

        protected override NorthwindContext CreateDataSource()
        {
            return NorthwindContext.Instance;
        }
    }
}
