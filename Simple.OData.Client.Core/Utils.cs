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

        public static bool NamesAreEqual(string actualName, string requestedName, IPluralizer pluralizer)
        {
            return actualName.Homogenize() == requestedName.Homogenize()
                   || pluralizer != null && actualName.Homogenize() == pluralizer.Singularize(requestedName).Homogenize()
                   || pluralizer != null && actualName.Homogenize() == pluralizer.Pluralize(requestedName).Homogenize();
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

        public static EntryDetails ParseEntryDetails(ISession session, EntityCollection entityCollection, IDictionary<string, object> entryData,
            Func<IDictionary<string, object>, string> resolveContentIdFunc = null)
        {
            var contentId = resolveContentIdFunc != null ? resolveContentIdFunc(entryData) : null;
            var entryDetails = new EntryDetails();

            foreach (var item in entryData)
            {
                if (session.Provider.GetMetadata().HasStructuralProperty(entityCollection.ActualName, item.Key))
                {
                    entryDetails.AddProperty(item.Key, item.Value);
                }
                else if (session.Provider.GetMetadata().HasNavigationProperty(entityCollection.ActualName, item.Key))
                {
                    if (session.Provider.GetMetadata().IsNavigationPropertyMultiple(entityCollection.ActualName, item.Key))
                    {
                        var collection = item.Value as IEnumerable<object>;
                        if (collection != null)
                        {
                            foreach (var element in collection)
                            {
                                entryDetails.AddLink(item.Key, element, contentId);
                            }
                        }
                    }
                    else
                    {
                        entryDetails.AddLink(item.Key, item.Value, contentId);
                    }
                }
                else
                {
                    throw new UnresolvableObjectException(item.Key, String.Format("No property or association found for {0}.", item.Key));
                }
            }

            return entryDetails;
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
