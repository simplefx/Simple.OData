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
            builder.EntitySet<WorkTaskModel>("WorkTaskModels");
            builder.EntitySet<WorkTaskAttachmentModel>("WorkTaskAttachmentModels");
            builder.EntitySet<WorkActivityReportModel>("WorkActivityReportModels");
            config.Routes.MapODataRoute("odata", "odata", builder.GetEdmModel());
        }
    }
}
