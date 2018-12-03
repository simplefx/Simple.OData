using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

#pragma warning disable 1591

namespace Simple.OData.Client.Extensions
{
    public static class MemberInfoExtensions
    {
        private static ConcurrentDictionary<MemberInfo, MappingInfo> cache = new ConcurrentDictionary<MemberInfo, MappingInfo>();

        public static bool IsNotMapped(this MemberInfo memberInfo)
        {
            return Helper(memberInfo).IsNotMapped;
        }

        /// <summary>
        /// Extract a column name from the member's attributes
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static string GetMappedName(this MemberInfo memberInfo)
        {
            return Helper(memberInfo).MappedName;
        }

        private static MappingInfo Helper(MemberInfo memberInfo)
        {
            var info = cache.GetOrAdd(memberInfo, MappingInfoFactory(memberInfo));

            return info;
        }

        private static MappingInfo MappingInfoFactory(MemberInfo memberInfo)
        {
            return new MappingInfo
            {
                IsNotMapped = memberInfo.IsNotMappedInternal(),
                MappedName = memberInfo.GetMappedNameInternal()
            };
        }

        private static bool IsNotMappedInternal(this MemberInfo member)
        {
            return member.GetCustomAttributes().Any(x => x.GetType().Name == "NotMappedAttribute");
        }

        private static string GetMappedNameInternal(this MemberInfo property)
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

    public class MappingInfo
    {
        public bool IsNotMapped { get; set; }

        public string MappedName { get; set; }
    }
}