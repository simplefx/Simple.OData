using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public static class CustomConverters
    {
        private static ConcurrentDictionary<string, ITypeConverter> _converters;
        
        static CustomConverters()
        {
            _converters = new ConcurrentDictionary<string, ITypeConverter>();
        }

        public static ITypeConverter Converter(string uri)
        {
            // TODO: Have a settings switch whether we use global dictionary or not?
            return _converters.GetOrAdd(uri, new TypeConverter());
        }

        public static ITypeConverter Global => Converter("global");

        [Obsolete("Use ODataClientSettings.TypeCache.RegisterTypeConverter")]
        public static void RegisterTypeConverter(Type type, Func<IDictionary<string, object>, object> converter)
        {
            Global.RegisterTypeConverter(type, converter);

            // Side-effect if we call the global is to register in all other converters
            foreach (var kvp in _converters)
            {
                if (kvp.Key != "global")
                {
                    kvp.Value.RegisterTypeConverter(type, converter);
                }
            }
        }

        [Obsolete("Use ODataClientSettings.TypeCache.RegisterTypeConverter")]
        public static void RegisterTypeConverter(Type type, Func<object, object> converter)
        {
            Global.RegisterTypeConverter(type, converter);

            // Side-effect if we call the global is to register in all other converters
            foreach (var kvp in _converters)
            {
                if (kvp.Key != "global")
                {
                    kvp.Value.RegisterTypeConverter(type, converter);
                }
            }
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static bool HasDictionaryConverter(Type type)
        {
            return Global.HasDictionaryConverter(type);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static bool HasObjectConverter(Type type)
        {
            return Global.HasObjectConverter(type);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static T Convert<T>(IDictionary<string, object> value)
        {
            return Global.Convert<T>(value);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static T Convert<T>(object value)
        {
            return Global.Convert<T>(value);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static object Convert(IDictionary<string, object> value, Type type)
        {
            return Global.Convert(value, type);
        }

        [Obsolete("Use ITypeCache.Converter")]
        public static object Convert(object value, Type type)
        {
            return Global.Convert(value, type);
        }
    }
}