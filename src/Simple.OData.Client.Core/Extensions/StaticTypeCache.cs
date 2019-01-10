using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    /// <summary>
    /// Just forwards to the static helpers
    /// </summary>
    [Obsolete("Use TypeCaches.Global")]
    public class StaticTypeCache : ITypeCache
    {
        private ConcurrentDictionary<Type, string> containerNames;

        /// <summary>
        /// Creates a new instance of the <see cref="StaticTypeCache"/> class.
        /// </summary>
        public StaticTypeCache()
        {
            containerNames = new ConcurrentDictionary<Type, string>();
        }

        /// <copydoc cref="ITypeCache.Converter" />

        public ITypeConverter Converter => CustomConverters.Global;

        /// <copydoc cref="ITypeCache.Register{T}" />
        public void Register<T>(string dynamicContainerName = "DynamicProperties")
        {
            Register(typeof(T), dynamicContainerName);
        }

        /// <copydoc cref="ITypeCache.Register" />
        public void Register(Type type, string dynamicContainerName = "DynamicProperties")
        {
            containerNames.GetOrAdd(type, dynamicContainerName);
        }

        /// <copydoc cref="ITypeCache.IsDynamicType" />
        [Obsolete("Use DynamicContainerName")]
        public bool IsDynamicType(Type type)
        {
            // NB Not really supported for the StaticTypeCache
            return false;
        }

        /// <copydoc cref="ITypeCache.DynamicContainerName" />
        public string DynamicContainerName(Type type)
        {
            return containerNames.TryGetValue(type, out var containerName) ? containerName : null;
        }

        /// <copydoc cref="ITypeCache.GetMappedProperties" />
        public IEnumerable<PropertyInfo> GetMappedProperties(Type type)
        {
            return type.GetMappedProperties();
        }

        /// <copydoc cref="ITypeCache.GetMappedPropertiesWithNames" />
        public IEnumerable<Tuple<PropertyInfo, string>> GetMappedPropertiesWithNames(Type type)
        {
            return type.GetMappedPropertiesWithNames();
        }

        /// <copydoc cref="ITypeCache.GetMappedProperty" />
        public PropertyInfo GetMappedProperty(Type type, string propertyName)
        {
            return type.GetMappedProperty(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetMappedName(Type, PropertyInfo)" />
        public string GetMappedName(Type type, PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMappedName();
        }

        /// <copydoc cref="ITypeCache.GetMappedName(Type, string)" />
        public string GetMappedName(Type type, string propertyName)
        {
            return type.GetNamedProperty(propertyName).GetMappedName();
        }

        /// <copydoc cref="ITypeCache.GetAllProperties" />
        public IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            return type.GetAllProperties();
        }

        /// <copydoc cref="ITypeCache.GetNamedProperty" />
        public PropertyInfo GetNamedProperty(Type type, string propertyName)
        {
            return type.GetNamedProperty(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredProperties" />
        public IEnumerable<PropertyInfo> GetDeclaredProperties(Type type)
        {
            return type.GetDeclaredProperties();
        }

        /// <copydoc cref="ITypeCache.GetDeclaredProperty" />
        public PropertyInfo GetDeclaredProperty(Type type, string propertyName)
        {
            return type.GetDeclaredProperty(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetAllFields" />
        public IEnumerable<FieldInfo> GetAllFields(Type type)
        {
            return type.GetAllFields();
        }

        /// <copydoc cref="ITypeCache.GetAnyField" />
        public FieldInfo GetAnyField(Type type, string fieldName, bool includeNonPublic = false)
        {
            return type.GetAnyField(fieldName, includeNonPublic);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredFields" />
        public IEnumerable<FieldInfo> GetDeclaredFields(Type type)
        {
            return type.GetDeclaredFields();
        }

        /// <copydoc cref="ITypeCache.GetDeclaredField" />
        public FieldInfo GetDeclaredField(Type type, string fieldName)
        {
            return type.GetDeclaredField(fieldName);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredMethod" />
        public MethodInfo GetDeclaredMethod(Type type, string methodName)
        {
            return type.GetDeclaredMethod(methodName);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredConstructors" />
        public IEnumerable<ConstructorInfo> GetDeclaredConstructors(Type type)
        {
            return type.GetDeclaredConstructors();
        }

        /// <copydoc cref="ITypeCache.GetDefaultConstructor" />
        public ConstructorInfo GetDefaultConstructor(Type type)
        {
            return type.GetDefaultConstructor();
        }

        /// <copydoc cref="ITypeCache.GetTypeAttributes" />
        public TypeAttributes GetTypeAttributes(Type type)
        {
            return type.GetTypeAttributes();
        }

        /// <copydoc cref="ITypeCache.GetGenericTypeArguments" />
        public Type[] GetGenericTypeArguments(Type type)
        {
            return type.GetGenericTypeArguments();
        }

        /// <copydoc cref="ITypeCache.IsAnonymousType" />
        public bool IsAnonymousType(Type type)
        {
            return type.IsAnonymousType();
        }

        /// <copydoc cref="ITypeCache.IsTypeAssignableFrom" />
        public bool IsTypeAssignableFrom(Type type, Type otherType)
        {
            return type.IsAssignableFrom(otherType);
        }

        /// <copydoc cref="ITypeCache.HasCustomAttribute" />
        public bool HasCustomAttribute(Type type, Type attributeType, bool inherit)
        {
            return type.HasCustomAttribute(attributeType, inherit);
        }

        /// <copydoc cref="ITypeCache.IsGeneric" />
        public bool IsGeneric(Type type)
        {
            return type.IsGeneric();
        }

        /// <copydoc cref="ITypeCache.IsValue" />
        public bool IsValue(Type type)
        {
            return type.IsValue();
        }

        /// <copydoc cref="ITypeCache.IsEnumType" />
        public bool IsEnumType(Type type)
        {
            return type.IsEnumType();
        }

        /// <copydoc cref="ITypeCache.GetBaseType" />
        public Type GetBaseType(Type type)
        {
            return type.GetBaseType();
        }

        /// <copydoc cref="ITypeCache.GetMappedName(Type)" />
        public string GetMappedName(Type type)
        {
            return type.GetMappedName();
        }
    }
}