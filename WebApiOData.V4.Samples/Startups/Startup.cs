using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using Owin;
using WebApiOData.V4.Samples.Models;

namespace WebApiOData.V4.Samples.Startups
{
    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            var config = new HttpConfiguration();

#if PRODUCTS
            config.MapODataServiceRoute(
                routeName: "functions route", 
                routePrefix: "functions", 
                model: FunctionStartup.GetEdmModel(config));
#endif
#if MOVIES
            config.MapODataServiceRoute(
                routeName: "actions route", 
                routePrefix: "actions", 
                model: ActionStartup.GetEdmModel(config));
#endif      

            builder.UseWebApi(config);
        }
    }
}