using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public class Utils
    {
        public static string StreamToString(Stream stream)
        {
            string result;

            using (var reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        public static Stream StringToStream(string str)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(str));
        }

        public static T CastExpressionWithTypeCheck<T>(Expression expression) where T : Expression
        {
            var typedExpression = expression as T;
            if (typedExpression == null)
                throw NotSupportedExpression(expression);
            return typedExpression;
        }

        public static Exception NotSupportedExpression(Expression expression)
        {
            return new NotSupportedException(String.Format("Not supported expression of type {0} ({1}): {2}",
                expression.GetType(), expression.NodeType, expression));
        }

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

#if NET40 || SILVERLIGHT || PORTABLE_LEGACY
        public static Task<T> GetTaskFromResult<T>(T result)
        {
            return TaskEx.FromResult(result);
        }
#else
        public static Task<T> GetTaskFromResult<T>(T result)
        {
            return Task.FromResult(result);
        }
#endif
    }
}
