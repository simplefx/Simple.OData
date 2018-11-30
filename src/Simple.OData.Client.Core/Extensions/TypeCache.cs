using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    public class TypeCache : ITypeCache
    {
        private ConcurrentDictionary<Type, TypeHelper> cache;

        public TypeCache()
        {
            cache = new ConcurrentDictionary<Type, TypeHelper>();
        }

        public void Register<T>(bool dynamicType = false, string dynamicContainerName = "DynamicProperties")
        {
            // TODO: Implicitly register subtypes as well?
            Register(typeof(T), dynamicType, dynamicContainerName);
        }

        public void Register(Type type, bool dynamicType = false, string dynamicContainerName = "DynamicProperties")
        {
            // TODO: Implicitly register subtypes as well?
            InternalRegister(type, dynamicType, dynamicContainerName);
        }

        public bool IsDynamicType(Type type)
        {
            return Helper(type).IsDynamicType;
        }

        public string DynamicPropertiesName(Type type)
        {
            return Helper(type).DynamicPropertiesName;
        }

        public IEnumerable<PropertyInfo> GetMappedProperties(Type type)
        {
            return Helper(type).MappedProperties;
        }

        public IEnumerable<Tuple<PropertyInfo, string>> GetMappedPropertiesWithNames(Type type)
        {
            return Helper(type).MappedPropertiesWithNames;
        }

        public PropertyInfo GetMappedProperty(Type type, string propertyName)
        {
            return Helper(type).GetMappedProperty(propertyName);
        }

        public string GetMappedName(Type type, PropertyInfo propertyInfo)
        {
            return Helper(type).GetMappedName(propertyInfo);
        }

        public string GetMappedName(Type type, string propertyName)
        {
            return Helper(type).GetMappedName(propertyName);
        }

        public IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            return Helper(type).AllProperties;
        }

        public PropertyInfo GetNamedProperty(Type type, string propertyName)
        {
            return Helper(type).GetAnyProperty(propertyName);
        }

        public IEnumerable<PropertyInfo> GetDeclaredProperties(Type type)
        {
            return Helper(type).DeclaredProperties;
        }

        public PropertyInfo GetDeclaredProperty(Type type, string propertyName)
        {
            return Helper(type).GetDeclaredProperty(propertyName);
        }

        public IEnumerable<FieldInfo> GetAllFields(Type type)
        {
            return Helper(type).AllFields;
        }

        public FieldInfo GetAnyField(Type type, string fieldName, bool includeNonPublic = false)
        {
            return Helper(type).GetAnyField(fieldName, includeNonPublic);
        }

        public IEnumerable<FieldInfo> GetDeclaredFields(Type type)
        {
            return Helper(type).DeclaredFields;
        }

        public FieldInfo GetDeclaredField(Type type, string fieldName)
        {
            return Helper(type).GetDeclaredField(fieldName);
        }

        public MethodInfo GetDeclaredMethod(Type type, string methodName)
        {
            return Helper(type).GetDeclaredMethod(methodName);
        }

        public IEnumerable<ConstructorInfo> GetDeclaredConstructors(Type type)
        {
            return Helper(type).GetDeclaredConstructors();
        }

        public ConstructorInfo GetDefaultConstructor(Type type)
        {
            return Helper(type).GetDefaultConstructor();
        }

        public TypeAttributes GetTypeAttributes(Type type)
        {
            return Helper(type).GetTypeAttributes();
        }

        public Type[] GetGenericTypeArguments(Type type)
        {
            return Helper(type).GetGenericTypeArguments();
        }

        public bool IsAnonymousType(Type type)
        {
            return Helper(type).IsAnonymousType;
        }

        public bool IsGeneric(Type type)
        {
            return Helper(type).IsGeneric;
        }

        public bool IsValue(Type type)
        {
            return Helper(type).IsValue;
        }

        public bool IsEnumType(Type type)
        {
            return Helper(type).IsEnumType;
        }

        public Type GetBaseType(Type type)
        {
            return Helper(type).BaseType;
        }

        public bool IsTypeAssignableFrom(Type type, Type otherType)
        {
            return Helper(type).IsTypeAssignableFrom(otherType);
        }

        public bool HasCustomAttribute(Type type, Type attributeType, bool inherit)
        {
            return Helper(type).HasCustomAttribute(attributeType, inherit);
        }

        public string GetMappedName(Type type)
        {
            return Helper(type).MappedName;
        }

        private TypeHelper Helper(Type type)
        {
            var helper = cache.GetOrAdd(type, x => InternalRegister(x));

            return helper;
        }

        private TypeHelper InternalRegister(Type type, bool dynamicType = false, string dynamicContainerName = "DynamicProperties")
        {
            var helper = new TypeHelper(type, dynamicType, dynamicContainerName);

            cache[type] = helper;

            return helper;
        }
    }
}
