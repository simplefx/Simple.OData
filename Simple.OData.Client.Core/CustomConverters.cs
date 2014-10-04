using System;
using System.Collections.Generic;

namespace Simple.OData.Client
{
    public static class CustomConverters
    {
        private static readonly Dictionary<Type, Func<IDictionary<string, object>, object>> _converters; 

        static CustomConverters()
        {
            _converters = new Dictionary<Type, Func<IDictionary<string, object>, object>>();
        }

        public static void RegisterTypeConverter(Type type, Func<IDictionary<string, object>, object> converter)
        {
            lock (_converters)
            {
                if (_converters.ContainsKey(type))
                {
                    _converters.Remove(type);
                }
                _converters.Add(type, converter);
            }
        }

        public static bool HasConverter<T>()
        {
            return HasConverter(typeof (T));
        }

        public static bool HasConverter(Type type)
        {
            return _converters.ContainsKey(type);
        }

        public static T Convert<T>(IDictionary<string, object> value)
        {
            return (T)Convert(value, typeof(T));
        }

        public static object Convert(IDictionary<string, object> value, Type type)
        {
            Func<IDictionary<string, object>, object> converter;
            if (_converters.TryGetValue(type, out converter))
            {
                return converter(value);
            }

            throw new InvalidOperationException(string.Format("No custom converter found for type {0}", type));
        }
    }
}