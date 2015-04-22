using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    static class DictionaryExtensions
    {
        private static readonly Dictionary<Type, ConstructorInfo> _constructors = new Dictionary<Type, ConstructorInfo>();

        internal static Func<IDictionary<string, object>, ODataEntry> CreateDynamicODataEntry { get; set; }

        public static T ToObject<T>(this IDictionary<string, object> source,
            string dynamicPropertiesContainerName = null, bool dynamicObject = false)
            where T : class
        {
            if (source == null)
                return default(T);
            if (typeof(IDictionary<string, object>).IsTypeAssignableFrom(typeof(T)))
                return source as T;
            if (typeof(T) == typeof(ODataEntry))
                return CreateODataEntry(source, dynamicObject) as T;

            return (T)ToObject(source, typeof(T), CreateInstance<T>, dynamicPropertiesContainerName, dynamicObject);
        }

        public static object ToObject(this IDictionary<string, object> source, Type type, string dynamicPropertiesContainerName = null)
        {
            var ctor = type.GetDefaultConstructor();
            if (ctor == null && !CustomConverters.HasConverter(type))
            {
                throw new InvalidOperationException(
                    string.Format("Unable to create an instance of type {0} that does not have a default constructor.", type.Name));
            }

            return ToObject(source, type, () => ctor.Invoke(new object[] { }), dynamicPropertiesContainerName, false);
        }

        private static object ToObject(this IDictionary<string, object> source, Type type, Func<object> instanceFactory,
            string dynamicPropertiesContainerName, bool dynamicObject)
        {
            if (source == null)
                return null;
            if (typeof(IDictionary<string, object>).IsTypeAssignableFrom(type))
                return source;
            if (type == typeof(ODataEntry))
                return CreateODataEntry(source, dynamicObject);

            if (CustomConverters.HasConverter(type))
            {
                return CustomConverters.Convert(source, type);
            }

            var instance = instanceFactory();

            Func<Type, bool> IsCompoundType = fieldOrPropertyType =>
                !fieldOrPropertyType.IsValue() && !fieldOrPropertyType.IsArray && fieldOrPropertyType != typeof(string);

            Func<Type, object, bool> IsCollectionType = (fieldOrPropertyType, itemValue) =>
                (fieldOrPropertyType.IsArray ||
                fieldOrPropertyType.IsGeneric() &&
                typeof(System.Collections.IEnumerable).IsTypeAssignableFrom(fieldOrPropertyType)) &&
                (itemValue as System.Collections.IEnumerable) != null;

            Func<Type, object, object> ConvertEnum = (fieldOrPropertyType, itemValue) =>
            {
                if (itemValue == null)
                    return null;
                var stringValue = itemValue.ToString();
                int intValue;
                if (int.TryParse(stringValue, out intValue))
                {
                    object result;
                    Utils.TryConvert(intValue, fieldOrPropertyType, out result);
                    return result;
                }
                else
                {
                    return Enum.Parse(fieldOrPropertyType, stringValue, false);
                }
            };

            Func<Type, object, object> ConvertSingle = (fieldOrPropertyType, itemValue) =>
                IsCompoundType(fieldOrPropertyType)
                ? itemValue.ToDictionary().ToObject(fieldOrPropertyType)
                : fieldOrPropertyType.IsEnumType()
                    ? ConvertEnum(fieldOrPropertyType, itemValue)
                    : itemValue;

            Func<Type, object, object> ConvertCollection = (fieldOrPropertyType, itemValue) =>
            {
                var elementType = fieldOrPropertyType.IsArray
                    ? fieldOrPropertyType.GetElementType()
                    : fieldOrPropertyType.IsGeneric() && fieldOrPropertyType.GetGenericTypeArguments().Length == 1
                        ? fieldOrPropertyType.GetGenericTypeArguments()[0]
                        : null;
                if (elementType == null)
                    return null;

                var count = (itemValue as System.Collections.IEnumerable).Cast<object>().Count();
                var arrayValue = Array.CreateInstance(elementType, count);

                count = 0;
                foreach (var item in (itemValue as System.Collections.IEnumerable))
                {
                    (arrayValue as Array).SetValue(ConvertSingle(elementType, item), count++);
                }

                if (fieldOrPropertyType.IsArray || fieldOrPropertyType.IsTypeAssignableFrom(arrayValue.GetType()))
                {
                    return arrayValue;
                }
                else
                {
                    var typedef = typeof(IEnumerable<>);
                    var enumerableType = typedef.MakeGenericType(elementType);
                    var ctor = fieldOrPropertyType.GetDeclaredConstructors().FirstOrDefault(
                        x => x.GetParameters().Length == 1 && x.GetParameters().First().ParameterType == enumerableType);
                    return ctor != null
                        ? ctor.Invoke(new object[] { arrayValue })
                        : null;
                }
            };

            Func<Type, object, object> ConvertValue = (fieldOrPropertyType, itemValue) =>
                IsCollectionType(fieldOrPropertyType, itemValue)
                ? ConvertCollection(fieldOrPropertyType, itemValue)
                : ConvertSingle(fieldOrPropertyType, itemValue);

            IDictionary<string, object> dynamicProperties = null;
            if (!string.IsNullOrEmpty(dynamicPropertiesContainerName))
            {
                dynamicProperties = CreateDynamicPropertiesContainer(type, instance, dynamicPropertiesContainerName);
            }

            foreach (var item in source)
            {
                var property = type.GetAnyProperty(item.Key) ??
                    type.GetAllProperties().FirstOrDefault(x => x.GetMappedName() == item.Key);

                if (property != null && property.CanWrite && !property.IsNotMapped())
                {
                    if (item.Value != null)
                        property.SetValue(instance, ConvertValue(property.PropertyType, item.Value), null);
                }
                else if (dynamicProperties != null)
                {
                    dynamicProperties.Add(item.Key, item.Value);
                }
            }

            return instance;
        }

        public static IDictionary<string, object> ToDictionary(this object source)
        {
            if (source == null)
                return new Dictionary<string, object>();
            if (source is IDictionary<string, object>)
                return source as IDictionary<string, object>;
            if (source is ODataEntry)
                return (Dictionary<string, object>)(source as ODataEntry);

            var properties = source.GetType().GetAllProperties();
            return properties.ToDictionary
            (
                x => x.GetMappedName(),
                x => x.GetValue(source, null)
            );
        }

        private static T CreateInstance<T>()
            where T : class
        {
            ConstructorInfo ctor = null;

            if (!_constructors.TryGetValue(typeof(T), out ctor))
            {
                if (typeof(T) == typeof(IDictionary<string, object>))
                {
                    return new Dictionary<string, object>() as T;
                }
                else
                {
                    ctor = typeof(T).GetDefaultConstructor();
                    if (ctor != null)
                    {
                        lock (_constructors)
                        {
                            if (!_constructors.ContainsKey(typeof(T)))
                                _constructors.Add(typeof(T), ctor);
                        }
                    }
                }
            }

            if (ctor == null)
            {
                throw new InvalidOperationException(
                    string.Format("Unable to create an instance of type {0} that does not have a default constructor.", typeof(T).Name));
            }

            return ctor.Invoke(new object[] { }) as T;
        }

        private static ODataEntry CreateODataEntry(IDictionary<string, object> source, bool dynamicObject = false)
        {
            return dynamicObject && CreateDynamicODataEntry != null ?
                CreateDynamicODataEntry(source) :
                new ODataEntry(source);
        }

        private static IDictionary<string, object> CreateDynamicPropertiesContainer(
            Type type, object instance, string dynamicPropertiesContainerName)
        {
            var property = type.GetAnyProperty(dynamicPropertiesContainerName);

            if (property == null)
                throw new ArgumentException(string.Format("Type {0} does not have property {1} ",
                    type, dynamicPropertiesContainerName));

            if (!typeof(IDictionary<string, object>).IsTypeAssignableFrom(property.PropertyType))
                throw new InvalidOperationException(
                    string.Format("Property {0} must implement IDictionary<string,object> interface",
                    dynamicPropertiesContainerName));

            var dynamicProperties = new Dictionary<string, object>();
            property.SetValue(instance, dynamicProperties, null);
            return dynamicProperties;
        }
    }
}
