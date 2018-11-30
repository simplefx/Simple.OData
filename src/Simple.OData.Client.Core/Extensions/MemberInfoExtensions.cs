using System.Linq;
using System.Reflection;

#pragma warning disable 1591

namespace Simple.OData.Client.Extensions
{
    public static class MemberInfoExtensions
    {
        public static bool IsNotMapped(this MemberInfo member)
        {
            return member.GetCustomAttributes().Any(x => x.GetType().Name == "NotMappedAttribute");
        }

        /// <summary>
        /// Extract a property name from the Member's attributes
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        /// <remarks>Talks directly to the type extensions as the result will be cached by the ITypeCache.GetMappedProperty, also has to handle fields/dynamic from Linq expressions</remarks>
        public static string GetMappedName(this MemberInfo property)
        {
            var supportedAttributeNames = new[]
            {
                "DataAttribute",
                "DataMemberAttribute",
                "ColumnAttribute",
            };

            var propertyName = property.Name;
            string attributeProperty;
            var mappingAttribute = property.GetCustomAttributes()
                                           .FirstOrDefault(x => supportedAttributeNames.Any(y => x.GetType().Name == y));
            if (mappingAttribute != null)
            {
                attributeProperty = "Name";
            }
            else
            {
                attributeProperty = "PropertyName";
                mappingAttribute = property.GetCustomAttributes()
                                           .FirstOrDefault(x => x.GetType().GetNamedProperty(attributeProperty) != null);
            }

            if (mappingAttribute != null)
            {
                var nameProperty = mappingAttribute.GetType().GetNamedProperty(attributeProperty);
                if (nameProperty != null)
                {
                    var propertyValue = nameProperty.GetValue(mappingAttribute, null);
                    if (propertyValue != null)
                        propertyName = propertyValue.ToString();
                }
            }

            return propertyName;
        }
    }
}