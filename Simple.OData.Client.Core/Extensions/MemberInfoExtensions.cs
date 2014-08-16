using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    public static class MemberInfoExtensions
    {
#if NET40 || SILVERLIGHT || PORTABLE_LEGACY
        public static IEnumerable<object> GetCustomAttributes(this MemberInfo member)
        {
            return member.GetCustomAttributes(true);
        }
#else
#endif

        public static bool IsNotMapped(this MemberInfo member)
        {
            return member.GetCustomAttributes().Any(x => x.GetType().Name == "NotMappedAttribute");
        }

        public static string GetMappedName(this PropertyInfo property)
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
                    .FirstOrDefault(x => x.GetType().GetAnyProperty(attributeName) != null);
            }

            if (mappingAttribute != null)
            {
                var nameProperty = mappingAttribute.GetType().GetAnyProperty(attributeName);
                if (nameProperty != null)
                {
                    propertyName = nameProperty.GetValue(mappingAttribute, null).ToString();
                }
            }

            return propertyName;
        }
    }
}