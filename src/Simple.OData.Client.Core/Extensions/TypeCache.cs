using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    /// <copydoc cref="ITypeCache" />
    public class TypeCache : ITypeCache
    {
        private readonly ConcurrentDictionary<Type, TypeHelper> cache;

        /// <summary>
        /// Creates a new instance of the <see cref="TypeCache"/> class.
        /// </summary>
        /// <param name="converter"></param>
        public TypeCache(ITypeConverter converter)
        {
            cache = new ConcurrentDictionary<Type, TypeHelper>();
            Converter = converter;
        }

        /// <copydoc cref="ITypeCache.Converter" />
        public ITypeConverter Converter { get; }

        /// <copydoc cref="ITypeCache.Register{T}" />
        public void Register<T>(string dynamicContainerName = "DynamicProperties")
        {
            Register(typeof(T), dynamicContainerName);
        }

        /// <copydoc cref="ITypeCache.Register" />
        public void Register(Type type, string dynamicContainerName = "DynamicProperties")
        {
            InternalRegister(type, true, dynamicContainerName);

            foreach (var subType in type.DerivedTypes())
            {
                InternalRegister(subType, true, dynamicContainerName);
            }
        }

        /// <copydoc cref="ITypeCache.IsDynamicType" />
        public bool IsDynamicType(Type type)
        {
            return Helper(type).IsDynamicType;
        }

        /// <copydoc cref="ITypeCache.DynamicContainerName" />
        public string DynamicContainerName(Type type)
        {
            return Helper(type).DynamicPropertiesName;
        }

        /// <copydoc cref="ITypeCache.GetMappedProperties" />
        public IEnumerable<PropertyInfo> GetMappedProperties(Type type)
        {
            return Helper(type).MappedProperties;
        }

        /// <copydoc cref="ITypeCache.GetMappedPropertiesWithNames" />
        public IEnumerable<Tuple<PropertyInfo, string>> GetMappedPropertiesWithNames(Type type)
        {
            return Helper(type).MappedPropertiesWithNames;
        }

        /// <copydoc cref="ITypeCache.GetMappedProperty" />
        public PropertyInfo GetMappedProperty(Type type, string propertyName)
        {
            return Helper(type).GetMappedProperty(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetMappedName(Type, PropertyInfo)" />
        public string GetMappedName(Type type, PropertyInfo propertyInfo)
        {
            return Helper(type).GetMappedName(propertyInfo);
        }

        /// <copydoc cref="ITypeCache.GetMappedName(Type, string)" />
        public string GetMappedName(Type type, string propertyName)
        {
            return Helper(type).GetMappedName(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetAllProperties" />
        public IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            return Helper(type).AllProperties;
        }

        /// <copydoc cref="ITypeCache.GetNamedProperty(Type, string)" />
        public PropertyInfo GetNamedProperty(Type type, string propertyName)
        {
            return Helper(type).GetAnyProperty(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredProperties" />
        public IEnumerable<PropertyInfo> GetDeclaredProperties(Type type)
        {
            return Helper(type).DeclaredProperties;
        }

        /// <copydoc cref="ITypeCache.GetDeclaredProperty" />
        public PropertyInfo GetDeclaredProperty(Type type, string propertyName)
        {
            return Helper(type).GetDeclaredProperty(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetAllFields" />
        public IEnumerable<FieldInfo> GetAllFields(Type type)
        {
            return Helper(type).AllFields;
        }

        /// <copydoc cref="ITypeCache.GetAnyField" />
        public FieldInfo GetAnyField(Type type, string fieldName, bool includeNonPublic = false)
        {
            return Helper(type).GetAnyField(fieldName, includeNonPublic);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredFields" />
        public IEnumerable<FieldInfo> GetDeclaredFields(Type type)
        {
            return Helper(type).DeclaredFields;
        }

        /// <copydoc cref="ITypeCache.GetDeclaredField" />
        public FieldInfo GetDeclaredField(Type type, string fieldName)
        {
            return Helper(type).GetDeclaredField(fieldName);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredMethod" />
        public MethodInfo GetDeclaredMethod(Type type, string methodName)
        {
            return Helper(type).GetDeclaredMethod(methodName);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredConstructors" />
        public IEnumerable<ConstructorInfo> GetDeclaredConstructors(Type type)
        {
            return Helper(type).GetDeclaredConstructors();
        }

        /// <copydoc cref="ITypeCache.GetDefaultConstructor" />
        public ConstructorInfo GetDefaultConstructor(Type type)
        {
            return Helper(type).GetDefaultConstructor();
        }

        /// <copydoc cref="ITypeCache.GetTypeAttributes" />
        public TypeAttributes GetTypeAttributes(Type type)
        {
            return Helper(type).GetTypeAttributes();
        }

        /// <copydoc cref="ITypeCache.GetGenericTypeArguments" />
        public Type[] GetGenericTypeArguments(Type type)
        {
            return Helper(type).GetGenericTypeArguments();
        }

        /// <copydoc cref="ITypeCache.IsAnonymousType" />
        public bool IsAnonymousType(Type type)
        {
            return Helper(type).IsAnonymousType;
        }

        /// <copydoc cref="ITypeCache.IsGeneric" />
        public bool IsGeneric(Type type)
        {
            return Helper(type).IsGeneric;
        }

        /// <copydoc cref="ITypeCache.IsValue" />
        public bool IsValue(Type type)
        {
            return Helper(type).IsValue;
        }

        /// <copydoc cref="ITypeCache.IsEnumType" />
        public bool IsEnumType(Type type)
        {
            return Helper(type).IsEnumType;
        }

        /// <copydoc cref="ITypeCache.GetBaseType" />
        public Type GetBaseType(Type type)
        {
            return Helper(type).BaseType;
        }

        /// <copydoc cref="ITypeCache.IsTypeAssignableFrom" />
        public bool IsTypeAssignableFrom(Type type, Type otherType)
        {
            return Helper(type).IsTypeAssignableFrom(otherType);
        }

        /// <copydoc cref="ITypeCache.HasCustomAttribute" />
        public bool HasCustomAttribute(Type type, Type attributeType, bool inherit)
        {
            return Helper(type).HasCustomAttribute(attributeType, inherit);
        }

        /// <copydoc cref="ITypeCache.GetMappedName(Type)" />
        public string GetMappedName(Type type)
        {
            return Helper(type).MappedName;
        }

        private TypeHelper Helper(Type type)
        {
            var helper = cache.GetOrAdd(type, x => InternalRegister(x));

            return helper;
        }

        private TypeHelper InternalRegister(Type type, bool dynamicType = false, string dynamicContainerName = null)
        {
            var helper = new TypeHelper(type, dynamicType, dynamicContainerName);

            cache[type] = helper;

            return helper;
        }
    }
}
