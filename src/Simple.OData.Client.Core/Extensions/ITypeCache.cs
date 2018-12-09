using System;
using System.Collections.Generic;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    public interface ITypeCache
    {
        /// <summary>
        /// Gets the type converters.
        /// </summary>
        ITypeConverter Converter { get; }

        /// <summary>
        /// Register the dynamic properties name, also applies to sub-types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dynamicContainerName"></param>
        void Register<T>(string dynamicContainerName = "DynamicProperties");

        /// <summary>
        /// Register the dynamic properties name, also applies to sub-types
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dynamicContainerName"></param>
        void Register(Type type, string dynamicContainerName = "DynamicProperties");

        bool IsDynamicType(Type type);

        string DynamicPropertiesName(Type type);

        IEnumerable<PropertyInfo> GetMappedProperties(Type type);

        IEnumerable<Tuple<PropertyInfo, string>> GetMappedPropertiesWithNames(Type type);

        /// <summary>
        /// Get a property that is either directly named or mapped via an attribute
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        PropertyInfo GetMappedProperty(Type type, string propertyName);

        /// <summary>
        /// Get the mapping name for a property
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        string GetMappedName(Type type, PropertyInfo propertyInfo);

        /// <summary>
        /// Get the mapping name for a named property
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        string GetMappedName(Type type, string propertyName);

        IEnumerable<PropertyInfo> GetAllProperties(Type type);

        /// <summary>
        /// Get a directly named property
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        PropertyInfo GetNamedProperty(Type type, string propertyName);

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
