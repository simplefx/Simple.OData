using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dispatcher;
using System.Web.OData;

namespace WebApiOData.V4.Samples
{
    public class CustomHttpControllerTypeResolver : DefaultHttpControllerTypeResolver
    {
        public CustomHttpControllerTypeResolver(Type controllerType)
            : base(IsController(controllerType))
        {
        }

        private static Predicate<Type> IsController(Type controllerType)
        {
            Predicate<Type> predicate = t =>
                t == typeof (MetadataController)
                || t == controllerType;

            return predicate;
        }
    }
}