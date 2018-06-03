using System;
using System.Collections.Generic;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public static class CustomConverters
    {
        private static readonly Dictionary<Type, Func<IDictionary<string, object>, object>> _dictionaryConverters;
        private static readonly Dictionary<Type, Func<object, object>> _objectConverters; 

        static CustomConverters()
        {
            _dictionaryConverters = new Dictionary<Type, Func<IDictionary<string, object>, object>>();
            _objectConverters = new Dictionary<Type, Func<object, object>>();
        }

        public static void RegisterTypeConverter(Type type, Func<IDictionary<string, object>, object> converter)
        {
            lock (_dictionaryConverters)
            {
                if (_dictionaryConverters.ContainsKey(type))
                {
                    _dictionaryConverters.Remove(type);
                }
                _dictionaryConverters.Add(type, converter);
            }
        }

        public static void RegisterTypeConverter(Type type, Func<object, object> converter)
        {
            lock (_objectConverters)
            {
                if (_objectConverters.ContainsKey(type))
                {
                    _objectConverters.Remove(type);
                }
                _objectConverters.Add(type, converter);
            }
        }

        public static bool HasDictionaryConverter<T>()
        {
            return HasDictionaryConverter(typeof (T));
        }

        public static bool HasDictionaryConverter(Type type)
        {
            return _dictionaryConverters.ContainsKey(type);
        }

        public static bool HasObjectConverter<T>()
        {
            return HasObjectConverter(typeof(T));
        }

        public static bool HasObjectConverter(Type type)
        {
            return _objectConverters.ContainsKey(type);
        }

        public static T Convert<T>(IDictionary<string, object> value)
        {
            return (T)Convert(value, typeof(T));
        }

        public static T Convert<T>(object value)
        {
            return (T)Convert(value, typeof(T));
        }

        public static object Convert(IDictionary<string, object> value, Type type)
        {
            Func<IDictionary<string, object>, object> converter;
            if (_dictionaryConverters.TryGetValue(type, out converter))
            {
                return converter(value);
            }

            throw new InvalidOperationException(string.Format("No custom converter found for type {0}", type));
        }

        public static object Convert(object value, Type type)
        {
            Func<object, object> converter;
            if (_objectConverters.TryGetValue(type, out converter))
            {
                return converter(value);
            }

            throw new InvalidOperationException(string.Format("No custom converter found for type {0}", type));
        }
    }
}