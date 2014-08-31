using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class Utils
    {
        public static string StreamToString(Stream stream)
        {
            stream.Position = 0;
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static Stream StringToStream(string str)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(str));
        }

        public static bool NamesAreEqual(string actualName, string requestedName)
        {
            return actualName.Homogenize() == requestedName.Homogenize()
                   || actualName.Homogenize() == requestedName.Singularize().Homogenize()
                   || actualName.Homogenize() == requestedName.Pluralize().Homogenize();
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

        public static IEnumerable<PropertyInfo> GetMappedProperties(Type type)
        {
            return type.GetAllProperties().Where(x => !x.IsNotMapped());
        }

        public static PropertyInfo GetMappedProperty(Type type, string propertyName)
        {
            var property = type.GetAnyProperty(propertyName);
            return property == null || property.IsNotMapped() ? null : property;
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
