using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Simple.OData.Client.Extensions
{
    internal static class DictionaryExtensions
    {
        public static T AsObjectOfType<T>(this IDictionary<string, object> source)
            where T : class, new()
        {
            var value = new T();
            var type = value.GetType();
            return (T)AsObjectOfType(source, type, value);
        }

        public static object AsObjectOfType(this IDictionary<string, object> source, Type type, object value = null)
        {
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
                return fieldOrPropertyType.IsArray && (itemValue as System.Collections.IEnumerable) != null;
            };

            Func<Type, object, object> ConvertSingle = (fieldOrPropertyType, itemValue) =>
            {
                return IsCompoundType(fieldOrPropertyType)
                    ? (itemValue as IDictionary<string, object>).AsObjectOfType(fieldOrPropertyType)
                    : itemValue;
            };

            Func<Type, object, object> ConvertCollection = (fieldOrPropertyType, itemValue) =>
            {
                var elementType = fieldOrPropertyType.GetElementType();
                var count = 0;
                foreach (var v in (itemValue as System.Collections.IEnumerable)) count++;
                var arrayValue = Array.CreateInstance(elementType, count);

                count = 0;
                foreach (var item in (itemValue as System.Collections.IEnumerable))
                {
                    (arrayValue as Array).SetValue(ConvertSingle(elementType, item), count++);
                }
                return arrayValue;
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

        public static IDictionary<string, object> AsDictionary(this object source,
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );

        }
    }
}
