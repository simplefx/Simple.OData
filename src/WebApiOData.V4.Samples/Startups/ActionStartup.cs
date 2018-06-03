using System;
using System.Web.Http;
using System.Web.OData.Batch;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using WebApiOData.V4.Samples.Controllers;
using WebApiOData.V4.Samples.Models;

namespace WebApiOData.V4.Samples.Startups
{
    public class ActionStartup : Startup
    {
        public ActionStartup()
            : base(typeof(MoviesController))
        {
        }

        protected override void ConfigureController(HttpConfiguration config)
        {
            config.MapODataServiceRoute(
                routeName: "OData actions",
                routePrefix: "actions",
                model: GetEdmModel(config),
                batchHandler: new DefaultODataBatchHandler(new HttpServer(config)));
        }

        private static IEdmModel GetEdmModel(HttpConfiguration config)
        {
            var modelBuilder = new ODataConventionModelBuilder(config);
            var moviesEntitySet = modelBuilder.EntitySet<Movie>("Movies");

            // Now add actions.

            // CheckOut
            // URI: ~/odata/Movies(1)/ODataActionsSample.Models.CheckOut
            ActionConfiguration checkOutAction = modelBuilder.EntityType<Movie>().Action("CheckOut");
            checkOutAction.ReturnsFromEntitySet<Movie>("Movies");

            // ReturnMovie
            // URI: ~/odata/Movies(1)/ODataActionsSample.Models.Return
            // Binds to a single entity; no parameters.
            ActionConfiguration returnAction = modelBuilder.EntityType<Movie>().Action("Return");
            returnAction.ReturnsFromEntitySet<Movie>("Movies");

            // CheckOutMany action
            // URI: ~/odata/Movies/ODataActionsSample.Models.CheckOutMany
            // Binds to a collection of entities.  This action accepts a collection of parameters.
            ActionConfiguration checkOutManyAction = modelBuilder.EntityType<Movie>().Collection.Action("CheckOutMany");
            checkOutManyAction.CollectionParameter<int>("MovieIDs");
            checkOutManyAction.ReturnsCollectionFromEntitySet<Movie>("Movies");

            // CreateMovie action
            // URI: ~/odata/CreateMovie
            // Unbound action. It is invoked from the service root.
            ActionConfiguration createMovieAction = modelBuilder.Action("CreateMovie");
            createMovieAction.Parameter<string>("Title");
            createMovieAction.ReturnsFromEntitySet<Movie>("Movies");

            modelBuilder.Namespace = typeof(Movie).Namespace;
            return modelBuilder.GetEdmModel();
        }
    }
}