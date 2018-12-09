using System;
using System.Collections.Generic;


namespace Simple.OData.Client
{
    public class TypeConverters : ITypeConverter
    {
        private readonly Dictionary<Type, Func<IDictionary<string, object>, object>> _dictionaryConverters;
        private readonly Dictionary<Type, Func<object, object>> _objectConverters;

        public TypeConverters()
        {
            _dictionaryConverters = new Dictionary<Type, Func<IDictionary<string, object>, object>>();
            _objectConverters = new Dictionary<Type, Func<object, object>>();
        }

        public void RegisterTypeConverter(Type type, Func<IDictionary<string, object>, object> converter)
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

        public void RegisterTypeConverter(Type type, Func<object, object> converter)
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

        public bool HasDictionaryConverter<T>()
        {
            return HasDictionaryConverter(typeof (T));
        }

        public bool HasDictionaryConverter(Type type)
        {
            return _dictionaryConverters.ContainsKey(type);
        }

        public bool HasObjectConverter<T>()
        {
            return HasObjectConverter(typeof(T));
        }

        public bool HasObjectConverter(Type type)
        {
            return _objectConverters.ContainsKey(type);
        }

        public T Convert<T>(IDictionary<string, object> value)
        {
            return (T)Convert(value, typeof(T));
        }

        public T Convert<T>(object value)
        {
            return (T)Convert(value, typeof(T));
        }

        public object Convert(IDictionary<string, object> value, Type type)
        {
            if (_dictionaryConverters.TryGetValue(type, out var converter))
            {
                return converter(value);
            }

            throw new InvalidOperationException($"No custom converter found for type {type}");
        }

        public object Convert(object value, Type type)
        {
            if (_objectConverters.TryGetValue(type, out var converter))
            {
                return converter(value);
            }

            throw new InvalidOperationException($"No custom converter found for type {type}");
        }
    }
}
