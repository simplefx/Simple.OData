using System.Web.Http;
using System.Web.Http.OData.Builder;
using Simple.OData.ProductService.Models;

namespace Simple.OData.ProductService.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Product>("Products");
            config.Routes.MapODataRoute("odata", "odata", builder.GetEdmModel());
        }
    }
}
