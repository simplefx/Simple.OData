using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public sealed class EdmType : IEquatable<EdmType>
    {
        public static readonly EdmType Binary = new EdmType("Edm.Binary");
        public static readonly EdmType Boolean = new EdmType("Edm.Boolean");
        public static readonly EdmType Byte = new EdmType("Edm.Byte");
        public static readonly EdmType DateTime = new EdmType("Edm.DateTime");
        public static readonly EdmType DateTimeOffset = new EdmType("Edm.DateTimeOffset");
        public static readonly EdmType Decimal = new EdmType("Edm.Decimal");
        public static readonly EdmType Double = new EdmType("Edm.Double");
        public static readonly EdmType Float = new EdmType("Edm.Float");
        public static readonly EdmType Guid = new EdmType("Edm.Guid");
        public static readonly EdmType Int16 = new EdmType("Edm.Int16");
        public static readonly EdmType Int32 = new EdmType("Edm.Int32");
        public static readonly EdmType Int64 = new EdmType("Edm.Int64");
        public static readonly EdmType SByte = new EdmType("Edm.SByte");
        public static readonly EdmType Single = new EdmType("Edm.Single");
        public static readonly EdmType String = new EdmType("Edm.String");
        public static readonly EdmType Time = new EdmType("Edm.Time");
        public static readonly EdmType Geography = new EdmType("Edm.Geography");
        public static readonly EdmType GeographyPoint = new EdmType("Edm.GeographyPoint");
        public static readonly EdmType GeographyLineString = new EdmType("Edm.GeographyLineString");
        public static readonly EdmType GeographyPolygon = new EdmType("Edm.GeographyPolygon");
        public static readonly EdmType GeographyCollection = new EdmType("Edm.GeographyCollection");
        public static readonly EdmType GeographyMultiPoint = new EdmType("Edm.GeographyMultiPoint");
        public static readonly EdmType GeographyMultiLineString = new EdmType("Edm.GeographyMultiLineString");
        public static readonly EdmType GeographyMultiPolygon = new EdmType("Edm.GeographyMultiPolygon");
        public static readonly EdmType Geometry = new EdmType("Edm.Geometry");
        public static readonly EdmType GeometryPoint = new EdmType("Edm.GeometryPoint");
        public static readonly EdmType GeometryLineString = new EdmType("Edm.GeometryLineString");
        public static readonly EdmType GeometryPolygon = new EdmType("Edm.GeometryPolygon");
        public static readonly EdmType GeometryCollection = new EdmType("Edm.GeometryCollection");
        public static readonly EdmType GeometryMultiPoint = new EdmType("Edm.GeometryMultiPoint");
        public static readonly EdmType GeometryMultiLineString = new EdmType("Edm.GeometryMultiLineString");
        public static readonly EdmType GeometryMultiPolygon = new EdmType("Edm.GeometryMultiPolygon");
        public static readonly EdmType Stream = new EdmType("Edm.Stream");

        private static readonly Dictionary<Type, EdmType> EdmTypeMap = new Dictionary<Type, EdmType>
        {
            { typeof(Byte[]), Binary },
            { typeof(Boolean), Boolean },
            { typeof(Byte), Byte },
            { typeof(DateTime), DateTime },
            { typeof(DateTimeOffset), DateTimeOffset },
            { typeof(Decimal), Decimal },
            { typeof(Double), Double },
            { typeof(Guid), Guid },
            { typeof(Int16), Int16 },
            { typeof(Int32), Int32 },
            { typeof(Int64), Int64 },
            { typeof(SByte), SByte },
            { typeof(Single), Single },
            { typeof(String), String },
            { typeof(TimeSpan), Time },
        };

        private readonly string _text;

        public string Name
        {
            get { return _text;  }
        }

        private EdmType(string text)
        {
            this._text = text;
        }

        public static EdmType Parse(string s)
        {
            var result = TryParse(s);

            if (!result.Item1) throw new ArgumentOutOfRangeException();

            return result.Item2;
        }

        public static Tuple<bool, EdmType> TryParse(string s)
        {
            s = s.EnsureStartsWith("Edm.");

            var edmType = EnumerateTypes().FirstOrDefault(et => et._text == s);

            return Tuple.Create(edmType != null, edmType);
        }

        public static EdmType FromSystemType(Type systemType)
        {
            if (EdmTypeMap.ContainsKey(systemType))
            {
                return EdmTypeMap[systemType];
            }

            return EdmType.String;
        }

        public override string ToString()
        {
            return _text;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            return Equals(obj as EdmType);
        }

        public override int GetHashCode()
        {
            return _text.GetHashCode();
        }

        public bool Equals(EdmType other)
        {
            if (other == null) return false;

            return other._text.Equals(_text);
        }

        private static IEnumerable<EdmType> EnumerateTypes()
        {
            var edmTypes = from field in GetTypeFields()
                           where field.FieldType == typeof(EdmType)
                           select ((EdmType)field.GetValue(null));
            return edmTypes;
        }

        private static IEnumerable<FieldInfo> GetTypeFields()
        {
            return typeof(EdmType).GetDeclaredFields();
        }
    }
}
