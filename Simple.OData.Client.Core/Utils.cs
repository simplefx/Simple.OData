using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    static class Utils
    {
        public static string StreamToString(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return new StreamReader(stream).ReadToEnd();
        }

        public static Stream StringToStream(string str)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(str));
        }

        public static Stream CloneStream(Stream stream)
        {
            stream.Position = 0;
            var clonedStream = new MemoryStream();
            stream.CopyTo(clonedStream);
            return clonedStream;
        }

        public static bool NamesMatch(string actualName, string requestedName, IPluralizer pluralizer)
        {
            actualName = actualName.Split('.').Last();
            requestedName = requestedName.Split('.').Last();

            return actualName.Homogenize() == requestedName.Homogenize()
                   || pluralizer != null && actualName.Homogenize() == pluralizer.Singularize(requestedName).Homogenize()
                   || pluralizer != null && actualName.Homogenize() == pluralizer.Pluralize(requestedName).Homogenize();
        }

        public static T BestMatch<T>(this IEnumerable<T> collection, 
            Func<T, string> fieldFunc, string value, IPluralizer pluralizer)
            where T : class
        {
            return collection.FirstOrDefault(x => fieldFunc(x).Homogenize() == value.Homogenize())
                ?? collection.FirstOrDefault(x => NamesMatch(fieldFunc(x), value, pluralizer));
        }

        public static T BestMatch<T>(this IEnumerable<T> collection, 
            Func<T, bool> condition, Func<T, string> fieldFunc, string value, 
            IPluralizer pluralizer)
            where T : class
        {
            return collection.FirstOrDefault(x => fieldFunc(x).Homogenize() == value.Homogenize() && condition(x))
                ?? collection.FirstOrDefault(x => NamesMatch(fieldFunc(x), value, pluralizer) && condition(x));
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

        public static Uri CreateAbsoluteUri(string urlBase, string relativePath)
        {
            string url = string.IsNullOrEmpty(urlBase) ? "http://" : urlBase;
            if (!url.EndsWith("/"))
                url += "/";
            return new Uri(url + relativePath);
        }

        public static string ExtractCollectionName(string commandText)
        {
            var uri = new Uri(commandText, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
            {
                return uri.LocalPath.Split('/').Last();
            }
            else
            {
                return commandText.Split('?', '(', '/').First();
            }
        }

        public static bool TryConvert(object value, Type targetType, out object result)
        {
            try
            {
                if (value == null)
                {
                    if (targetType.IsValue())
                        result = Activator.CreateInstance(targetType);
                    else
                        result = null;
                }
                else if (targetType.IsEnumType() && value.GetType() == typeof(string))
                {
                    result = Enum.Parse(targetType, value.ToString(), true);
                }
                else if (targetType == typeof(DateTime) && value.GetType() == typeof(DateTimeOffset))
                {
                    result = ((DateTimeOffset) value).DateTime;
                }
                else if (targetType == typeof(DateTimeOffset) && value.GetType() == typeof(DateTime))
                {
                    result = new DateTimeOffset((DateTime)value);
                }
                else
                {
                    result = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                }
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
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
