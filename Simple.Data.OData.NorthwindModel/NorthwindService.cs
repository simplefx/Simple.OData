using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;

namespace Simple.Data.OData.NorthwindModel
{
    public class NorthwindService : DataService<NorthwindEntities>
    {
        public static void InitializeService(DataServiceConfiguration config)
        {
            Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");

            config.SetEntitySetAccessRule("*", EntitySetRights.All);
            config.SetServiceOperationAccessRule("*", ServiceOperationRights.All);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3;
            config.UseVerboseErrors = true;
        }

        protected override void HandleException(HandleExceptionArgs args)
        {
            base.HandleException(args);
        }

        [WebGet]
        public int ParseInt(string number)
        {
            return int.Parse(number);
        }
    }
}

