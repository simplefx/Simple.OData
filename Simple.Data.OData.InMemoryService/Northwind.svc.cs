using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using Elmah;
using Simple.Data.OData.NorthwindModel;

namespace Simple.Data.OData.InMemoryService
{
    public class Northwind : DataService<NorthwindContext>
    {
        public static void InitializeService(DataServiceConfiguration config)
        {
            config.SetEntitySetAccessRule("*", EntitySetRights.All);
            config.SetServiceOperationAccessRule("*", ServiceOperationRights.All);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
            config.UseVerboseErrors = true;
        }

        protected override void HandleException(HandleExceptionArgs args)
        {
            base.HandleException(args);
            Elmah.ErrorLog.GetDefault(null).Log(new Error(args.Exception));
        }

        protected override NorthwindContext CreateDataSource()
        {
            try
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Error(new Exception(NorthwindContext.Instance.Categories.Count().ToString())));
                return NorthwindContext.Instance;
            }
            catch (Exception exception)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Error(exception));
                throw;
            }
        }
    }
}
