using System;
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

        public static bool IsPropertyExcludedFromMapping(PropertyInfo property)
        {
            return property.GetCustomAttributes().Any(x => x.GetType().Name == "NotMappedAttribute");
        }

        public static string MapPropertyName(PropertyInfo property)
        {
            string attributeName;
            var propertyName = property.Name;
            var mappingAttribute = property.GetCustomAttributes()
                .FirstOrDefault(x => x.GetType().Name == "DataAttribute" || x.GetType().Name == "ColumnAttribute");
            if (mappingAttribute != null)
            {
                attributeName = "Name";
            }
            else
            {
                attributeName = "PropertyName";
                mappingAttribute = property.GetCustomAttributes()
                    .FirstOrDefault(x => x.GetType().GetAllProperties().Any(y => y.Name == attributeName));
            }

            if (mappingAttribute != null)
            {
                var nameProperty = mappingAttribute.GetType().GetAllProperties().SingleOrDefault(x => x.Name == attributeName);
                if (nameProperty != null)
                {
                    propertyName = nameProperty.GetValue(mappingAttribute, null).ToString();
                }
            }

            return propertyName;
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
