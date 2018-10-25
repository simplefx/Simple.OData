using System;
using System.Collections.Generic;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    public interface ITypeCache
    {
        bool IsDynamicType(Type type);

        string DynamicPropertiesName(Type type);

        IEnumerable<PropertyInfo> GetMappedProperties(Type type);

        PropertyInfo GetMappedProperty(Type type, string propertyName);

        IEnumerable<PropertyInfo> GetAllProperties(Type type);

        PropertyInfo GetAnyProperty(Type type, string propertyName);

        IEnumerable<PropertyInfo> GetDeclaredProperties(Type type);

        PropertyInfo GetDeclaredProperty(Type type, string propertyName);

        IEnumerable<FieldInfo> GetAllFields(Type type);

        FieldInfo GetAnyField(Type type, string fieldName, bool includeNonPublic = false);

        IEnumerable<FieldInfo> GetDeclaredFields(Type type);

        FieldInfo GetDeclaredField(Type type, string fieldName);

        MethodInfo GetDeclaredMethod(Type type, string methodName);

        IEnumerable<ConstructorInfo> GetDeclaredConstructors(Type type);

        ConstructorInfo GetDefaultConstructor(Type type);

        TypeAttributes GetTypeAttributes(Type type);

        Type[] GetGenericTypeArguments(Type type);

        bool IsAnonymousType(Type type);

        bool IsTypeAssignableFrom(Type type, Type otherType);

        bool HasCustomAttribute(Type type, Type attributeType, bool inherit);

        bool IsGeneric(Type type);

        bool IsValue(Type type);

        bool IsEnumType(Type type);

        Type GetBaseType(Type type);

        string GetMappedName(Type type);
    }
}
