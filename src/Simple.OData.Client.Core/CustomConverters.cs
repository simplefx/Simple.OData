using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public static class CustomConverters
    {
        private static ConcurrentDictionary<string, ITypeConverter> _converters;
        private static readonly ITypeConverter _converter;
        
        static CustomConverters()
        {
            _converter = new TypeConverters();

            // TODO: Have a global switch whether we use the dictionary or not
            _converters = new ConcurrentDictionary<string, ITypeConverter>();
        }

        public static ITypeConverter Converter(string uri)
        {
            return _converter;
            //return _converters.GetOrAdd(uri, new TypeConverters());
        }

        public static ITypeConverter Converters => _converter;

        [Obsolete("Use ITypeCache.Converter")]
        public static void RegisterTypeConverter(Type type, Func<IDictionary<string, object>, object> converter)
        {
            _converter.RegisterTypeConverter(type, converter);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static void RegisterTypeConverter(Type type, Func<object, object> converter)
        {
            _converter.RegisterTypeConverter(type, converter);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static bool HasDictionaryConverter(Type type)
        {
            return _converter.HasDictionaryConverter(type);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static bool HasObjectConverter(Type type)
        {
            return _converter.HasObjectConverter(type);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static T Convert<T>(IDictionary<string, object> value)
        {
            return _converter.Convert<T>(value);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static T Convert<T>(object value)
        {
            return _converter.Convert<T>(value);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static object Convert(IDictionary<string, object> value, Type type)
        {
            return _converter.Convert(value, type);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static object Convert(object value, Type type)
        {
            return _converter.Convert(value, type);
        }
    }
}