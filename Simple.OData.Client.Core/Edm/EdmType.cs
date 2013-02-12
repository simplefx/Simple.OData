using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Simple.OData.Client.Extensions;

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
        public static readonly EdmType Guid = new EdmType("Edm.Guid");
        public static readonly EdmType Int16 = new EdmType("Edm.Int16");
        public static readonly EdmType Int32 = new EdmType("Edm.Int32");
        public static readonly EdmType Int64 = new EdmType("Edm.Int64");
        public static readonly EdmType SByte = new EdmType("Edm.SByte");
        public static readonly EdmType Single = new EdmType("Edm.Single");
        public static readonly EdmType String = new EdmType("Edm.String");
        public static readonly EdmType Time = new EdmType("Edm.Time");

        private static readonly Dictionary<Type, EdmType> EdmTypeMap = new Dictionary<Type, EdmType>
        {
            { typeof(byte[]), Binary },
            { typeof(bool), Boolean },
            { typeof(byte), Byte },
            { typeof(DateTime), DateTime },
            { typeof(DateTimeOffset), DateTimeOffset },
            { typeof(double), Double },
            { typeof(decimal), Decimal },
            { typeof(Guid), Guid },
            { typeof(Int16), Int16 },
            { typeof(Int32), Int32 },
            { typeof(Int64), Int64 },
            { typeof(SByte), SByte },
            { typeof(Single), Single },
            { typeof(string), String },
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
            return typeof(EdmType).GetFields(BindingFlags.Public | BindingFlags.Static);
        }
    }
}
