using System;
using System.Threading.Tasks;

namespace Simple.Data.OData
{
    public static class Utils
    {
        public static T ExecuteAndUnwrap<T>(Func<Task<T>> func)
        {
            try
            {
                return func().Result;
            }
            catch (AggregateException exception)
            {
                throw UnwrapException(exception);
            }
        }

        public static void ExecuteAndUnwrap(Func<Task> func)
        {
            try
            {
                func().Wait();
            }
            catch (AggregateException exception)
            {
                throw UnwrapException(exception);
            }
        }

        private static Exception UnwrapException(Exception exception)
        {
            while (exception is AggregateException)
            {
                exception = exception.InnerException;
            }
            return exception;
        }
    }
}