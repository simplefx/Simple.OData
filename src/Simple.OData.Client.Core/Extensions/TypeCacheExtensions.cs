using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Simple.OData.Client.Extensions
{
    static class TypeCacheExtensions
    {
        public static IDictionary<string, object> ToDictionary(this ITypeCache typeCache, object value)
        {
            var mpn = typeCache.GetMappedPropertiesWithNames(value.GetType());

            return mpn.Select(x => new KeyValuePair<string, object>(x.Item2, x.Item1.GetValue(value, null)))
                      .ToIDictionary();
        }

        public static T Convert<T>(this ITypeCache typeCache, object value)
        {
            return (T) typeCache.Convert(value, typeof(T));
        }

        public static object Convert(this ITypeCache typeCache, object value, Type targetType)
        {
            if (value == null && !typeCache.IsValue(targetType))
                return null;
            else if (typeCache.TryConvert(value, targetType, out var result))
                return result;

            throw new FormatException($"Unable to convert value from type {value.GetType()} to type {targetType}");
        }

        public static bool TryConvert(this ITypeCache typeCache, object value, Type targetType, out object result)
        {
            try
            {
                if (value == null)
                {
                    if (typeCache.IsValue(targetType))
                        result = Activator.CreateInstance(targetType);
                    else
                        result = null;
                }
                else if (typeCache.IsTypeAssignableFrom(targetType, value.GetType()))
                {
                    result = value;
                }
                else if (targetType == typeof(string))
                {
                    result = value.ToString();
                }
                else if (typeCache.IsEnumType(targetType) && value is string)
                {
                    result = Enum.Parse(targetType, value.ToString(), true);
                }
                else if (targetType == typeof(byte[]) && value is string)
                {
                    result = System.Convert.FromBase64String(value.ToString());
                }
                else if (targetType == typeof(string) && value is byte[] bytes)
                {
                    result = System.Convert.ToBase64String(bytes);
                }
                else if ((targetType == typeof(DateTime) || targetType == typeof(DateTime?)) && value is DateTimeOffset offset)
                {
                    result = offset.DateTime;
                }
                else if ((targetType == typeof(DateTimeOffset) || targetType == typeof(DateTimeOffset?)) && value is DateTime time)
                {
                    result = new DateTimeOffset(time);
                }
                else if (typeCache.IsEnumType(targetType))
                {
                    result = Enum.ToObject(targetType, value);
                }
                else if (targetType == typeof(Guid) && value is string)
                {
                    result = new Guid(value.ToString());
                }
                else if (Nullable.GetUnderlyingType(targetType) != null)
                {
                    result = typeCache.Convert(value, Nullable.GetUnderlyingType(targetType));
                }
                else
                {
                    result = System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                }
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }
    }
}
