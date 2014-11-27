using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Owin;

namespace WebApiOData.V4.Samples
{
    public abstract class Startup
    {
        private readonly Type _controllerType;

        protected Startup(Type controllerType)
        {
            _controllerType = controllerType;
        }

        protected abstract void ConfigureController(HttpConfiguration config);

        public void Configuration(IAppBuilder builder)
        {
            var config = new HttpConfiguration();

            config.Services.Replace(
                typeof(IHttpControllerTypeResolver),
                new CustomHttpControllerTypeResolver(_controllerType));

            ConfigureController(config);

            builder.UseWebApi(config);
        }
    }
}