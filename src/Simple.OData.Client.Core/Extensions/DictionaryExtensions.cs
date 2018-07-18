using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    static class DictionaryExtensions
    {
        private static ConcurrentDictionary<Type, ActivatorDelegate> _defaultActivators = new ConcurrentDictionary<Type, ActivatorDelegate>();
        private static ConcurrentDictionary<Tuple<Type,Type>, ActivatorDelegate> _collectionActivators = new ConcurrentDictionary<Tuple<Type,Type>, ActivatorDelegate>();

        internal static Func<IDictionary<string, object>, ODataEntry> CreateDynamicODataEntry { get; set; }

        internal static void ClearCache()
        {
            _defaultActivators = new ConcurrentDictionary<Type, ActivatorDelegate>();
            _collectionActivators = new ConcurrentDictionary<Tuple<Type,Type>, ActivatorDelegate>();
        }

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
            if (typeof(T) == typeof(string) || typeof(T).IsValue())
                throw new InvalidOperationException($"Unable to convert structural data to {typeof(T).Name}.");

            return (T)ToObject(source, typeof(T), dynamicPropertiesContainerName, dynamicObject);
        }

        public static object ToObject(this IDictionary<string, object> source, Type type, string dynamicPropertiesContainerName = null)
        {
            return ToObject(source, type, dynamicPropertiesContainerName, false);
        }

        private static object ToObject(this IDictionary<string, object> source, Type type, string dynamicPropertiesContainerName, bool dynamicObject)
        {
            if (source == null)
                return null;
            if (typeof(IDictionary<string, object>).IsTypeAssignableFrom(type))
                return source;
            if (type == typeof(ODataEntry))
                return CreateODataEntry(source, dynamicObject);

            if (CustomConverters.HasDictionaryConverter(type))
            {
                return CustomConverters.Convert(source, type);
            }

            var instance = CreateInstance(type);

            IDictionary<string, object> dynamicProperties = null;
            if (!string.IsNullOrEmpty(dynamicPropertiesContainerName))
            {
                dynamicProperties = CreateDynamicPropertiesContainer(type, instance, dynamicPropertiesContainerName);
            }

            foreach (var item in source)
            {
                var property = FindMatchingProperty(type, item);

                if (property != null && property.CanWrite && !property.IsNotMapped())
                {
                    if (item.Value != null)
                    {
                        property.SetValue(instance, ConvertValue(property.PropertyType, item.Value), null);
                    }
                }
                else
                {
                    dynamicProperties?.Add(item.Key, item.Value);
                }
            }

            return instance;
        }

        private static PropertyInfo FindMatchingProperty(Type type, KeyValuePair<string, object> item)
        {
            var property = type.GetAnyProperty(item.Key) ?? 
                type.GetAllProperties().FirstOrDefault(x => !x.IsNotMapped() && x.GetMappedName() == item.Key);

            if (property == null && item.Key == FluentCommand.AnnotationsLiteral)
            {
                property = type.GetAllProperties().FirstOrDefault(x => x.PropertyType == typeof(ODataEntryAnnotations));
            }

            return property;
        }

        private static object ConvertValue(Type type, object itemValue)
        {
            return IsCollectionType(type, itemValue)
                ? ConvertCollection(type, itemValue)
                : ConvertSingle(type, itemValue);
        }

        private static bool IsCollectionType(Type type, object itemValue)
        {
            return 
                (type.IsArray || type.IsGeneric() &&
                typeof(System.Collections.IEnumerable).IsTypeAssignableFrom(type)) &&
                (itemValue as System.Collections.IEnumerable) != null;
        }

        private static bool IsCompoundType(Type type)
        {
            return !type.IsValue() && !type.IsArray && type != typeof(string);
        }

        private static object ConvertEnum(Type type, object itemValue)
        {
            if (itemValue == null)
                return null;

            var stringValue = itemValue.ToString();
            if (int.TryParse(stringValue, out var intValue))
            {
                Utils.TryConvert(intValue, type, out var result);
                return result;
            }
            else
            {
                return Enum.Parse(type, stringValue, false);
            }
        }

        private static object ConvertSingle(Type type, object itemValue)
        {
            object TryConvert(object v, Type t) => Utils.TryConvert(v, t, out var result) ? result : v;

            return type == typeof(ODataEntryAnnotations)
                ? itemValue
                : IsCompoundType(type)
                    ? itemValue.ToDictionary().ToObject(type)
                    : type.IsEnumType()
                        ? ConvertEnum(type, itemValue)
                        : TryConvert(itemValue, type);
        }

        private static object ConvertCollection(Type type, object itemValue)
        {
            var elementType = type.IsArray
                ? type.GetElementType()
                : type.IsGeneric() && type.GetGenericTypeArguments().Length == 1
                    ? type.GetGenericTypeArguments()[0]
                    : null;

            if (elementType == null)
                return null;

            var count = (itemValue as System.Collections.IEnumerable).Cast<object>().Count();
            var arrayValue = Array.CreateInstance(elementType, count);

            count = 0;
            foreach (var item in (itemValue as System.Collections.IEnumerable))
            {
                arrayValue.SetValue(ConvertSingle(elementType, item), count++);
            }

            if (type.IsArray || type.IsTypeAssignableFrom(arrayValue.GetType()))
            {
                return arrayValue;
            }
            else
            {
                var collectionType = typeof(IEnumerable<>).MakeGenericType(elementType);
                var activator = _collectionActivators.GetOrAdd(new Tuple<Type, Type>(type, collectionType), t => type.CreateActivator(collectionType));
                return activator?.Invoke(arrayValue);
            }
        }

        public static IDictionary<string, object> ToDictionary(this object source)
        {
            if (source == null)
                return new Dictionary<string, object>();
            if (source is IDictionary<string, object> objects)
                return objects;
            if (source is ODataEntry entry)
                return (Dictionary<string, object>)entry;

            var properties = Utils.GetMappedProperties(source.GetType());
            return properties.ToDictionary
            (
                x => x.GetMappedName(),
                x => x.GetValue(source, null)
            );
        }

        private static object CreateInstance(Type type)
        {
            if (type == typeof(IDictionary<string, object>))
            {
                return new Dictionary<string, object>();
            }
            else
            {
                var ctor = _defaultActivators.GetOrAdd(type, t => t.CreateActivator());
                return ctor.Invoke();
            }
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
                throw new ArgumentException($"Type {type} does not have property {dynamicPropertiesContainerName} ");

            if (!typeof(IDictionary<string, object>).IsTypeAssignableFrom(property.PropertyType))
                throw new InvalidOperationException($"Property {dynamicPropertiesContainerName} must implement IDictionary<string,object> interface");

            var dynamicProperties = new Dictionary<string, object>();
            property.SetValue(instance, dynamicProperties, null);
            return dynamicProperties;
        }
    }
}
