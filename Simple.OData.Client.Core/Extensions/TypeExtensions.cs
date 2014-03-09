using System;
using System.Collections.Generic;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    public static class TypeExtensions
    {
#if NET40 || SILVERLIGHT || PORTABLE_LEGACY
        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type)
        {
            return type.GetProperties();
        }

        public static PropertyInfo GetDeclaredProperty(this Type type, string propertyName)
        {
            return type.GetProperty(propertyName);
        }

        public static IEnumerable<FieldInfo> GetDeclaredFields(this Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Static);
        }
#else
        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type)
        {
            return type.GetTypeInfo().DeclaredProperties;
        }

        public static PropertyInfo GetDeclaredProperty(this Type type, string propertyName)
        {
            return type.GetTypeInfo().GetDeclaredProperty(propertyName);
        }

        public static IEnumerable<FieldInfo> GetDeclaredFields(this Type type)
        {
            return typeof(EdmType).GetTypeInfo().DeclaredFields;
        }
#endif
    }
}
