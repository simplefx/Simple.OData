using System;
using System.Collections.Generic;

namespace Simple.OData.Client
{
    public interface ITypeConverter
    {
        /// <summary>
        /// Register a dictionary type converter
        /// </summary>
        /// <param name="converter"></param>
        void RegisterTypeConverter<T>(Func<IDictionary<string, object>, object> converter);

        /// <summary>
        /// Register an object type converter
        /// </summary>
        /// <param name="converter"></param>
        void RegisterTypeConverter<T>(Func<object, object> converter);

        /// <summary>
        /// Register a dictionary type converter
        /// </summary>
        /// <param name="type"></param>
        /// <param name="converter"></param>
        void RegisterTypeConverter(Type type, Func<IDictionary<string, object>, object> converter);

        /// <summary>
        /// Register an object type converter
        /// </summary>
        /// <param name="type"></param>
        /// <param name="converter"></param>
        void RegisterTypeConverter(Type type, Func<object, object> converter);

        /// <summary>
        /// Determine if a type has a dictionary converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool HasDictionaryConverter<T>();

        /// <summary>
        /// Determine if a type has a dictionary converter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool HasDictionaryConverter(Type type);

        /// <summary>
        /// Determine if a type has an object converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool HasObjectConverter<T>();

        /// <summary>
        /// Determine if a type has an object converter
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool HasObjectConverter(Type type);

        /// <summary>
        /// Convert a dictionary to the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        T Convert<T>(IDictionary<string, object> value);

        /// <summary>
        /// Convert an object to the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        T Convert<T>(object value);

        /// <summary>
        /// Convert a dictionary to the specified type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        object Convert(IDictionary<string, object> value, Type type);

        /// <summary>
        /// Convert an object to the specified type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        object Convert(object value, Type type);
    }
}
