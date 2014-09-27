using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpatialV3 = System.Spatial;
using SpatialV4 = Microsoft.Spatial;

namespace Simple.OData.Client.Extensions
{
    static class DictionaryExtensions
    {
        private static readonly Dictionary<Type, ConstructorInfo> _constructors = new Dictionary<Type, ConstructorInfo>();

        internal static Func<IDictionary<string, object>, ODataEntry> CreateDynamicODataEntry { get; set; }

        public static T ToObject<T>(this IDictionary<string, object> source, bool dynamicObject = false)
            where T : class
        {
            if (source == null)
                return default(T);
            if (typeof(IDictionary<string, object>).IsTypeAssignableFrom(typeof(T)))
                return source as T;
            if (typeof(T) == typeof(ODataEntry))
                return CreateODataEntry(source, dynamicObject) as T;

            if (typeof(T) == typeof(SpatialV3.GeographyPoint))
                return CreateGeographyPointV3(source) as T;
            if (typeof(T) == typeof(SpatialV4.GeographyPoint))
                return CreateGeographyPointV3(source) as T;
            if (typeof(T) == typeof(SpatialV3.GeometryPoint))
                return CreateGeometryPointV4(source) as T;
            if (typeof(T) == typeof(SpatialV4.GeometryPoint))
                return CreateGeometryPointV4(source) as T;

            var value = CreateInstance<T>();
            var type = value.GetType();
            return (T)ToObject(source, type, value, dynamicObject);
        }

        public static object ToObject(this IDictionary<string, object> source, Type type, object value = null, bool dynamicObject = false)
        {
            if (source == null)
                return null;
            if (typeof(IDictionary<string, object>).IsTypeAssignableFrom(type))
                return source;
            if (type == typeof(ODataEntry))
                return CreateODataEntry(source, dynamicObject);

            if (type == typeof(SpatialV3.GeographyPoint))
                return CreateGeographyPointV3(source);
            if (type == typeof(SpatialV4.GeographyPoint))
                return CreateGeographyPointV4(source);
            if (type == typeof(SpatialV3.GeometryPoint))
                return CreateGeometryPointV3(source);
            if (type == typeof(SpatialV4.GeometryPoint))
                return CreateGeometryPointV4(source);

            if (value == null)
            {
                var defaultConstructor = type.GetDefaultConstructor();
                if (defaultConstructor != null)
                {
                    value = defaultConstructor.Invoke(new object[] { });
                }
            }

            Func<Type, bool> IsCompoundType = fieldOrPropertyType => 
                !fieldOrPropertyType.IsValue() && !fieldOrPropertyType.IsArray && fieldOrPropertyType != typeof(string);

            Func<Type, object, bool> IsCollectionType = (fieldOrPropertyType, itemValue) => 
                (fieldOrPropertyType.IsArray ||
                fieldOrPropertyType.IsGeneric() && 
                typeof(System.Collections.IEnumerable).IsTypeAssignableFrom(fieldOrPropertyType)) && 
                (itemValue as System.Collections.IEnumerable) != null;

            Func<Type, object, object> ConvertEnum = (fieldOrPropertyType, itemValue) =>
            {
                if (itemValue == null)
                    return null;
                var stringValue = itemValue.ToString();
                int intValue;
                if (int.TryParse(stringValue, out intValue))
                {
                    return Convert.ChangeType(intValue, fieldOrPropertyType, null);
                }
                else
                {
                    return Enum.Parse(fieldOrPropertyType, stringValue, false);
                }
            };

            Func<Type, object, object> ConvertSingle = (fieldOrPropertyType, itemValue) => 
                IsCompoundType(fieldOrPropertyType)
                ? itemValue.ToDictionary().ToObject(fieldOrPropertyType)
                : fieldOrPropertyType.IsEnumType()
                    ? ConvertEnum(fieldOrPropertyType, itemValue)
                    : itemValue;

            Func<Type, object, object> ConvertCollection = (fieldOrPropertyType, itemValue) =>
            {
                var elementType = fieldOrPropertyType.IsArray
                    ? fieldOrPropertyType.GetElementType()
                    : fieldOrPropertyType.IsGeneric() && fieldOrPropertyType.GetGenericTypeArguments().Length == 1
                        ? fieldOrPropertyType.GetGenericTypeArguments()[0]
                        : null;
                if (elementType == null)
                    return null;

                var count = (itemValue as System.Collections.IEnumerable).Cast<object>().Count();
                var arrayValue = Array.CreateInstance(elementType, count);

                count = 0;
                foreach (var item in (itemValue as System.Collections.IEnumerable))
                {
                    (arrayValue as Array).SetValue(ConvertSingle(elementType, item), count++);
                }

                if (fieldOrPropertyType.IsArray || fieldOrPropertyType.IsTypeAssignableFrom(arrayValue.GetType()))
                {
                    return arrayValue;
                }
                else
                {
                    var typedef = typeof (IEnumerable<>);
                    var enumerableType = typedef.MakeGenericType(elementType);
                    var ctor = fieldOrPropertyType.GetDeclaredConstructors().FirstOrDefault(
                        x => x.GetParameters().Length == 1 && x.GetParameters().First().ParameterType == enumerableType);
                    return ctor != null 
                        ? ctor.Invoke(new object[] { arrayValue}) 
                        : null;
                }
            };

            Func<Type, object, object> ConvertValue = (fieldOrPropertyType, itemValue) => 
                IsCollectionType(fieldOrPropertyType, itemValue)
                ? ConvertCollection(fieldOrPropertyType, itemValue)
                : ConvertSingle(fieldOrPropertyType, itemValue);

            foreach (var item in source)
            {
                if (item.Value != null)
                {
                    var property = type.GetAnyProperty(item.Key) ?? 
                        type.GetAllProperties().FirstOrDefault(x => x.GetMappedName() == item.Key);

                    if (property != null && property.CanWrite && !property.IsNotMapped())
                    {
                        property.SetValue(value, ConvertValue(property.PropertyType, item.Value), null);
                    }
                }
            }

            return value;
        }

        public static IDictionary<string, object> ToDictionary(this object source)
        {
            if (source == null)
                return new Dictionary<string, object>();
            if (source is IDictionary<string, object>)
                return source as IDictionary<string, object>;
            if (source is ODataEntry)
                return (Dictionary<string, object>)(source as ODataEntry);

            var properties = source.GetType().GetAllProperties();
            return properties.ToDictionary
            (
                x => x.GetMappedName(),
                x => x.GetValue(source, null)
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
                    ctor = typeof(T).GetDefaultConstructor();
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

        private static T GetValueOrDefault<T>(this IDictionary<string, object> source, string key)
        {
            object value;
            if (source.TryGetValue(key, out value))
            {
                return (T)value;
            }
            else
            {
                return default(T);
            }
        }

        private static SpatialV3.GeographyPoint CreateGeographyPointV3(IDictionary<string, object> source)
        {
            return SpatialV3.GeographyPoint.Create(
                SpatialV3.CoordinateSystem.Geography(source.ContainsKey("CoordinateSystem")
                    ? source.GetValueOrDefault<SpatialV3.CoordinateSystem>("CoordinateSystem").EpsgId
                    : null), 
                source.GetValueOrDefault<double>("Latitude"),
                source.GetValueOrDefault<double>("Longitude"),
                source.GetValueOrDefault<double?>("Z"),
                source.GetValueOrDefault<double?>("M"));
        }

        private static SpatialV4.GeographyPoint CreateGeographyPointV4(IDictionary<string, object> source)
        {
            return SpatialV4.GeographyPoint.Create(
                SpatialV4.CoordinateSystem.Geography(source.ContainsKey("CoordinateSystem")
                    ? source.GetValueOrDefault<SpatialV4.CoordinateSystem>("CoordinateSystem").EpsgId
                    : null),
                source.GetValueOrDefault<double>("Latitude"),
                source.GetValueOrDefault<double>("Longitude"),
                source.GetValueOrDefault<double?>("Z"),
                source.GetValueOrDefault<double?>("M"));
        }

        private static SpatialV3.GeometryPoint CreateGeometryPointV3(IDictionary<string, object> source)
        {
            return SpatialV3.GeometryPoint.Create(
                SpatialV3.CoordinateSystem.Geometry(source.ContainsKey("CoordinateSystem")
                    ? source.GetValueOrDefault<SpatialV3.CoordinateSystem>("CoordinateSystem").EpsgId
                    : null),
                source.GetValueOrDefault<double>("Latitude"),
                source.GetValueOrDefault<double>("Longitude"),
                source.GetValueOrDefault<double?>("Z"),
                source.GetValueOrDefault<double?>("M"));
        }

        private static SpatialV4.GeometryPoint CreateGeometryPointV4(IDictionary<string, object> source)
        {
            return SpatialV4.GeometryPoint.Create(
                SpatialV4.CoordinateSystem.Geometry(source.ContainsKey("CoordinateSystem")
                    ? source.GetValueOrDefault<SpatialV4.CoordinateSystem>("CoordinateSystem").EpsgId
                    : null),
                source.GetValueOrDefault<double>("Latitude"),
                source.GetValueOrDefault<double>("Longitude"),
                source.GetValueOrDefault<double?>("Z"),
                source.GetValueOrDefault<double?>("M"));
        }
    }
}
