using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    public class TypeHelper
    {
        public TypeHelper(Type type, bool dynamicType = false, string dynamicContainerName = "DynamicProperties")
        {
            Type = type;
            IsDynamicType = dynamicType;
            DynamicPropertiesName = dynamicContainerName;
            TypeInfo = type.GetTypeInfo();
            AllProperties = type.GetAllProperties().ToList();
            DeclaredProperties = type.GetDeclaredProperties().ToList();
            AllFields = type.GetAllFields().ToList();
            DeclaredFields = type.GetDeclaredFields().ToList();
            MappedName = type.GetMappedName();
            MappedProperties = type.GetMappedProperties().ToList();

            IsAnonymousType = type.IsAnonymousType();
        }

        public Type Type { get; set; }

        public TypeInfo TypeInfo { get; }

        public string MappedName { get; }

        public bool IsDynamicType { get; }

        public bool IsAnonymousType { get; }

        public bool IsGeneric => TypeInfo.IsGenericType;

        public bool IsValue => TypeInfo.IsValueType;

        public bool IsEnumType => TypeInfo.IsEnum;

        public Type BaseType => TypeInfo.BaseType;

        // TODO: Store this as a PropertyInfo?
        public string DynamicPropertiesName { get; set; }

        public IList<PropertyInfo> AllProperties { get; }

        public IList<PropertyInfo> DeclaredProperties { get; }

        public IList<FieldInfo> AllFields { get; }

        public IList<FieldInfo> DeclaredFields { get; }

        public IList<PropertyInfo> MappedProperties { get; }

        public PropertyInfo GetMappedProperty(string propertyName)
        {
            return MappedProperties.FirstOrDefault(x => x.Name == propertyName);
        }

        public PropertyInfo GetAnyProperty(string propertyName)
        {
            var currentType = Type;
            while (currentType != null && currentType != typeof(object))
            {
                var property = currentType.GetTypeInfo().GetDeclaredProperty(propertyName);
                if (property != null)
                    return property;

                currentType = currentType.GetTypeInfo().BaseType;
            }
            return null;
        }

        public PropertyInfo GetDeclaredProperty(string propertyName)
        {
            return TypeInfo.GetDeclaredProperty(propertyName);
        }

        public FieldInfo GetAnyField(string fieldName, bool includeNonPublic = false)
        {
            var currentType = Type;
            while (currentType != null && currentType != typeof(object))
            {
                var field = currentType.GetDeclaredField(fieldName);
                if (field != null)
                    return field;

                currentType = currentType.GetTypeInfo().BaseType;
            }
            return null;
        }

        public FieldInfo GetDeclaredField(string fieldName)
        {
            return TypeInfo.GetDeclaredField(fieldName);
        }

        public MethodInfo GetDeclaredMethod(string methodName)
        {
            return TypeInfo.GetDeclaredMethod(methodName);
        }

        public IEnumerable<ConstructorInfo> GetDeclaredConstructors()
        {
            return TypeInfo.DeclaredConstructors.Where(x => !x.IsStatic);
        }

        public ConstructorInfo GetDefaultConstructor()
        {
            return GetDeclaredConstructors().SingleOrDefault(x => x.GetParameters().Length == 0);
        }

        public TypeAttributes GetTypeAttributes()
        {
            return TypeInfo.Attributes;
        }

        public Type[] GetGenericTypeArguments()
        {
            return TypeInfo.GenericTypeArguments;
        }

        public bool IsTypeAssignableFrom(Type otherType)
        {
            return TypeInfo.IsAssignableFrom(otherType.GetTypeInfo());
        }

        public bool HasCustomAttribute(Type attributeType, bool inherit)
        {
            return TypeInfo.GetCustomAttribute(attributeType, inherit) != null;
        }
    }
}
