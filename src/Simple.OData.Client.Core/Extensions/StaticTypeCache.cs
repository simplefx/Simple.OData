using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    /// <summary>
    /// Just forwards to the static helpers
    /// </summary>
    public class StaticTypeCache : ITypeCache
    {
        private ConcurrentDictionary<Type, string> containerNames;

        public ITypeConverter Converter => CustomConverters.Converters;

        public StaticTypeCache()
        {
            containerNames = new ConcurrentDictionary<Type, string>();
        }

        public void Register<T>(string dynamicContainerName = "DynamicProperties")
        {
            Register(typeof(T), dynamicContainerName);
        }

        public void Register(Type type, string dynamicContainerName = "DynamicProperties")
        {
            containerNames.GetOrAdd(type, dynamicContainerName);
        }

        public bool IsDynamicType(Type type)
        {
            // NB Not really supported for the StaticTypeCache
            return false;
        }

        public string DynamicPropertiesName(Type type)
        {
            return containerNames.TryGetValue(type, out var containerName) ? containerName : null;
        }

        public IEnumerable<PropertyInfo> GetMappedProperties(Type type)
        {
            return type.GetMappedProperties();
        }

        public IEnumerable<Tuple<PropertyInfo, string>> GetMappedPropertiesWithNames(Type type)
        {
            return type.GetMappedPropertiesWithNames();
        }

        public PropertyInfo GetMappedProperty(Type type, string propertyName)
        {
            return type.GetMappedProperty(propertyName);
        }

        public string GetMappedName(Type type, PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMappedName();
        }

        public string GetMappedName(Type type, string propertyName)
        {
            return type.GetNamedProperty(propertyName).GetMappedName();
        }

        public IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            return type.GetAllProperties();
        }

        public PropertyInfo GetNamedProperty(Type type, string propertyName)
        {
            return type.GetNamedProperty(propertyName);
        }

        public IEnumerable<PropertyInfo> GetDeclaredProperties(Type type)
        {
            return type.GetDeclaredProperties();
        }

        public PropertyInfo GetDeclaredProperty(Type type, string propertyName)
        {
            return type.GetDeclaredProperty(propertyName);
        }

        public IEnumerable<FieldInfo> GetAllFields(Type type)
        {
            return type.GetAllFields();
        }

        public FieldInfo GetAnyField(Type type, string fieldName, bool includeNonPublic = false)
        {
            return type.GetAnyField(fieldName, includeNonPublic);
        }

        public IEnumerable<FieldInfo> GetDeclaredFields(Type type)
        {
            return type.GetDeclaredFields();
        }

        public FieldInfo GetDeclaredField(Type type, string fieldName)
        {
            return type.GetDeclaredField(fieldName);
        }

        public MethodInfo GetDeclaredMethod(Type type, string methodName)
        {
            return type.GetDeclaredMethod(methodName);
        }

        public IEnumerable<ConstructorInfo> GetDeclaredConstructors(Type type)
        {
            return type.GetDeclaredConstructors();
        }

        public ConstructorInfo GetDefaultConstructor(Type type)
        {
            return type.GetDefaultConstructor();
        }

        public TypeAttributes GetTypeAttributes(Type type)
        {
            return type.GetTypeAttributes();
        }

        public Type[] GetGenericTypeArguments(Type type)
        {
            return type.GetGenericTypeArguments();
        }

        public bool IsAnonymousType(Type type)
        {
            return type.IsAnonymousType();
        }

        public bool IsTypeAssignableFrom(Type type, Type otherType)
        {
            return type.IsAssignableFrom(otherType);
        }

        public bool HasCustomAttribute(Type type, Type attributeType, bool inherit)
        {
            return type.HasCustomAttribute(attributeType, inherit);
        }

        public bool IsGeneric(Type type)
        {
            return type.IsGeneric();
        }

        public bool IsValue(Type type)
        {
            return type.IsValue();
        }

        public bool IsEnumType(Type type)
        {
            return type.IsEnumType();
        }

        public Type GetBaseType(Type type)
        {
            return type.GetBaseType();
        }

        public string GetMappedName(Type type)
        {
            return type.GetMappedName();
        }
    }
}