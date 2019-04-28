using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    /// <copydoc cref="ITypeCache" />
    public class TypeCache : ITypeCache
    {
        private readonly ConcurrentDictionary<Type, TypeCacheResolver> _cache;
        private readonly INameMatchResolver _nameMatchResolver;

        /// <summary>
        /// Creates a new instance of the <see cref="TypeCache"/> class.
        /// </summary>
        /// <param name="converter"></param>
        public TypeCache(ITypeConverter converter, INameMatchResolver nameMatchResolver)
        {
            _cache = new ConcurrentDictionary<Type, TypeCacheResolver>();
            _nameMatchResolver = nameMatchResolver ?? ODataNameMatchResolver.Strict;
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
            return Resolver(type).IsDynamicType;
        }

        /// <copydoc cref="ITypeCache.DynamicContainerName" />
        public string DynamicContainerName(Type type)
        {
            return Resolver(type).DynamicPropertiesName;
        }

        /// <copydoc cref="ITypeCache.GetMappedProperties" />
        public IEnumerable<PropertyInfo> GetMappedProperties(Type type)
        {
            return Resolver(type).MappedProperties;
        }

        /// <copydoc cref="ITypeCache.GetMappedPropertiesWithNames" />
        public IEnumerable<Tuple<PropertyInfo, string>> GetMappedPropertiesWithNames(Type type)
        {
            return Resolver(type).MappedPropertiesWithNames;
        }

        /// <copydoc cref="ITypeCache.GetMappedProperty" />
        public PropertyInfo GetMappedProperty(Type type, string propertyName)
        {
            return Resolver(type).GetMappedProperty(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetMappedName(Type, PropertyInfo)" />
        public string GetMappedName(Type type, PropertyInfo propertyInfo)
        {
            return Resolver(type).GetMappedName(propertyInfo);
        }

        /// <copydoc cref="ITypeCache.GetMappedName(Type, string)" />
        public string GetMappedName(Type type, string propertyName)
        {
            return Resolver(type).GetMappedName(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetAllProperties" />
        public IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            return Resolver(type).AllProperties;
        }

        /// <copydoc cref="ITypeCache.GetNamedProperty(Type, string)" />
        public PropertyInfo GetNamedProperty(Type type, string propertyName)
        {
            return Resolver(type).GetAnyProperty(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredProperties" />
        public IEnumerable<PropertyInfo> GetDeclaredProperties(Type type)
        {
            return Resolver(type).DeclaredProperties;
        }

        /// <copydoc cref="ITypeCache.GetDeclaredProperty" />
        public PropertyInfo GetDeclaredProperty(Type type, string propertyName)
        {
            return Resolver(type).GetDeclaredProperty(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetAllFields" />
        public IEnumerable<FieldInfo> GetAllFields(Type type)
        {
            return Resolver(type).AllFields;
        }

        /// <copydoc cref="ITypeCache.GetAnyField" />
        public FieldInfo GetAnyField(Type type, string fieldName, bool includeNonPublic = false)
        {
            return Resolver(type).GetAnyField(fieldName, includeNonPublic);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredFields" />
        public IEnumerable<FieldInfo> GetDeclaredFields(Type type)
        {
            return Resolver(type).DeclaredFields;
        }

        /// <copydoc cref="ITypeCache.GetDeclaredField" />
        public FieldInfo GetDeclaredField(Type type, string fieldName)
        {
            return Resolver(type).GetDeclaredField(fieldName);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredMethod" />
        public MethodInfo GetDeclaredMethod(Type type, string methodName)
        {
            return Resolver(type).GetDeclaredMethod(methodName);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredConstructors" />
        public IEnumerable<ConstructorInfo> GetDeclaredConstructors(Type type)
        {
            return Resolver(type).GetDeclaredConstructors();
        }

        /// <copydoc cref="ITypeCache.GetDefaultConstructor" />
        public ConstructorInfo GetDefaultConstructor(Type type)
        {
            return Resolver(type).GetDefaultConstructor();
        }

        /// <copydoc cref="ITypeCache.GetTypeAttributes" />
        public TypeAttributes GetTypeAttributes(Type type)
        {
            return Resolver(type).GetTypeAttributes();
        }

        /// <copydoc cref="ITypeCache.GetGenericTypeArguments" />
        public Type[] GetGenericTypeArguments(Type type)
        {
            return Resolver(type).GetGenericTypeArguments();
        }

        /// <copydoc cref="ITypeCache.IsAnonymousType" />
        public bool IsAnonymousType(Type type)
        {
            return Resolver(type).IsAnonymousType;
        }

        /// <copydoc cref="ITypeCache.IsGeneric" />
        public bool IsGeneric(Type type)
        {
            return Resolver(type).IsGeneric;
        }

        /// <copydoc cref="ITypeCache.IsValue" />
        public bool IsValue(Type type)
        {
            return Resolver(type).IsValue;
        }

        /// <copydoc cref="ITypeCache.IsEnumType" />
        public bool IsEnumType(Type type)
        {
            return Resolver(type).IsEnumType;
        }

        /// <copydoc cref="ITypeCache.GetBaseType" />
        public Type GetBaseType(Type type)
        {
            return Resolver(type).BaseType;
        }

        /// <copydoc cref="ITypeCache.IsTypeAssignableFrom" />
        public bool IsTypeAssignableFrom(Type type, Type otherType)
        {
            return Resolver(type).IsTypeAssignableFrom(otherType);
        }

        /// <copydoc cref="ITypeCache.HasCustomAttribute" />
        public bool HasCustomAttribute(Type type, Type attributeType, bool inherit)
        {
            return Resolver(type).HasCustomAttribute(attributeType, inherit);
        }

        /// <copydoc cref="ITypeCache.GetMappedName(Type)" />
        public string GetMappedName(Type type)
        {
            return Resolver(type).MappedName;
        }

        private TypeCacheResolver Resolver(Type type)
        {
            var resolver = _cache.GetOrAdd(type, x => InternalRegister(x));

            return resolver;
        }

        private TypeCacheResolver InternalRegister(Type type, bool dynamicType = false, string dynamicContainerName = null)
        {
            var resolver = new TypeCacheResolver(type, _nameMatchResolver, dynamicType, dynamicContainerName);

            _cache[type] = resolver;

            return resolver;
        }
    }
}
