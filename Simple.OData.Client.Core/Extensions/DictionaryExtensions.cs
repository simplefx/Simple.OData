using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    internal static class DictionaryExtensions
    {
        private static readonly Dictionary<Type, ConstructorInfo> _constructors = new Dictionary<Type, ConstructorInfo>();

        internal static Func<IDictionary<string, object>, ODataEntry> CreateDynamicODataEntry { get; set; }

        public static T ToObject<T>(this IDictionary<string, object> source, bool dynamicObject = false)
            where T : class
        {
            if (source == null)
                return default(T);
            if (typeof (IDictionary<string, object>).IsAssignableFrom(typeof(T)))
                return source as T;
            if (typeof(T) == typeof(ODataEntry))
                return CreateODataEntry(source, dynamicObject) as T;

            var value = CreateInstance<T>();
            var type = value.GetType();
            return (T)ToObject(source, type, value, dynamicObject);
        }

        public static object ToObject(this IDictionary<string, object> source, Type type, object value = null, bool dynamicObject = false)
        {
            if (source == null)
                return null;
            if (typeof(IDictionary<string, object>).IsAssignableFrom(type))
                return source;
            if (type == typeof(ODataEntry))
                return CreateODataEntry(source, dynamicObject);

            if (value == null)
            {
                var defaultConstructor = type.GetConstructor(new Type[] {});
                if (defaultConstructor != null)
                {
                    value = defaultConstructor.Invoke(new object[] { });
                }
            }

            Func<Type, bool> IsCompoundType = fieldOrPropertyType =>
            {
                return !fieldOrPropertyType.IsValueType && !fieldOrPropertyType.IsArray && fieldOrPropertyType != typeof(string);
            };

            Func<Type, object, bool> IsCollectionType = (fieldOrPropertyType, itemValue) =>
            {
                return (fieldOrPropertyType.IsArray ||
                    fieldOrPropertyType.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(fieldOrPropertyType)) && 
                    (itemValue as System.Collections.IEnumerable) != null;
            };

            Func<Type, object, object> ConvertSingle = (fieldOrPropertyType, itemValue) =>
            {
                return IsCompoundType(fieldOrPropertyType)
                    ? (itemValue as IDictionary<string, object>).ToObject(fieldOrPropertyType)
                    : itemValue;
            };

            Func<Type, object, object> ConvertCollection = (fieldOrPropertyType, itemValue) =>
            {
                var elementType = fieldOrPropertyType.IsArray
                    ? fieldOrPropertyType.GetElementType()
                    : fieldOrPropertyType.IsGenericType && fieldOrPropertyType.GetGenericArguments().Length == 1
                        ? fieldOrPropertyType.GetGenericArguments()[0]
                        : null;
                if (elementType == null)
                    return null;

                var count = 0;
                foreach (var v in (itemValue as System.Collections.IEnumerable)) count++;
                var arrayValue = Array.CreateInstance(elementType, count);

                count = 0;
                foreach (var item in (itemValue as System.Collections.IEnumerable))
                {
                    (arrayValue as Array).SetValue(ConvertSingle(elementType, item), count++);
                }

                if (fieldOrPropertyType.IsArray)
                {
                    return arrayValue;
                }
                else
                {
                    var typedef = typeof (IEnumerable<>);
                    var enumerableType = typedef.MakeGenericType(elementType);
                    var ctor = fieldOrPropertyType.GetConstructor(new [] { enumerableType});
                    return ctor != null 
                        ? ctor.Invoke(new object[] { arrayValue}) 
                        : null;
                }
            };

            Func<Type, object, object> ConvertValue = (fieldOrPropertyType, itemValue) =>
            {
                return IsCollectionType(fieldOrPropertyType, itemValue)
                            ? ConvertCollection(fieldOrPropertyType, itemValue)
                            : ConvertSingle(fieldOrPropertyType, itemValue);
            };

            foreach (var item in source)
            {
                if (item.Value != null)
                {
                    var property = type.GetProperty(item.Key);
                    if (property != null)
                    {
                        property.SetValue(value, ConvertValue(property.PropertyType, item.Value), null);
                    }
                }
            }

            return value;
        }

        public static IDictionary<string, object> ToDictionary(this object source,
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            if (source == null)
                return new Dictionary<string, object>();
            if (source is IDictionary<string, object>)
                return source as IDictionary<string, object>;
            if (source is ODataEntry)
                return (Dictionary<string, object>)(source as ODataEntry);

            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
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
                    ctor = typeof(T).GetConstructor(new Type[] { });
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
    }
}
