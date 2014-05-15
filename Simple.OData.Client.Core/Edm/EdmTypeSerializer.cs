using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    static class EdmTypeSerializer
    {
        private static readonly Dictionary<EdmType, Func<string, object>> Readers = new Dictionary<EdmType, Func<string, object>>
        {
            { EdmType.Binary, ReadEdmBinary },
            { EdmType.Boolean, ReadEdmBoolean },
            { EdmType.Byte, ReadEdmByte },
            { EdmType.DateTime, ReadEdmDateTime },
            { EdmType.DateTimeOffset, ReadEdmDateTimeOffset },
            { EdmType.Decimal, ReadEdmDecimal },
            { EdmType.Double, ReadEdmDouble },
            { EdmType.Float, ReadEdmSingle },
            { EdmType.Guid, ReadEdmGuid },
            { EdmType.Int16, ReadEdmInt16 },
            { EdmType.Int32, ReadEdmInt32 },
            { EdmType.Int64, ReadEdmInt64 },
            { EdmType.SByte, ReadEdmSByte },
            { EdmType.Single, ReadEdmSingle },
            { EdmType.String, ReadEdmString },
            { EdmType.Time, ReadEdmTime },
        };

        private static readonly Dictionary<EdmType, Func<object, string>> Writers = new Dictionary<EdmType, Func<object, string>>
        {
            { EdmType.Binary, WriteEdmBinary },
            { EdmType.Boolean, WriteEdmBoolean },
            { EdmType.Byte, WriteEdmByte },
            { EdmType.DateTime, WriteEdmDateTime },
            { EdmType.DateTimeOffset, WriteEdmDateTimeOffset },
            { EdmType.Decimal, WriteEdmDecimal },
            { EdmType.Double, WriteEdmDouble },
            { EdmType.Float, WriteEdmSingle },
            { EdmType.Guid, WriteEdmGuid },
            { EdmType.Int16, WriteEdmInt16 },
            { EdmType.Int32, WriteEdmInt32 },
            { EdmType.Int64, WriteEdmInt64 },
            { EdmType.SByte, WriteEdmSByte },
            { EdmType.Single, WriteEdmSingle },
            { EdmType.String, WriteEdmString },
            { EdmType.Time, WriteEdmTime },
        };

        public static KeyValuePair<string, object> Read(XElement element, string elementName = null)
        {
            if (element == null) throw new ArgumentNullException("element");

            var typeAttribute = element.Attribute("m", "type").ValueOrDefault();
            object elementValue;
            if (element.Attribute("m", "null").ValueOrDefault() == "true")
            {
                elementValue = null;
            }
            else if (element.HasElements)
            {
                if (!string.IsNullOrEmpty(typeAttribute) && typeAttribute.StartsWith("Collection("))
                {
                    elementValue = ReadPropertyArray(element);
                }
                else
                {
                    elementValue = ReadPropertySet(element);
                }
            }
            else if (!string.IsNullOrEmpty(typeAttribute) && typeAttribute.StartsWith("Collection("))
            {
                elementValue = new object[0];
            }
            else
            {
                var reader = GetReader(element.Attribute("m", "type").ValueOrDefault());
                elementValue = reader(element.Value);
            }

            return new KeyValuePair<string, object>(elementName ?? element.Name.LocalName, elementValue);
        }

        private static object ReadPropertyArray(XElement element)
        {
            var properties = new List<object>();
            foreach (var propertyElement in element.Elements())
            {
                var kvp = Read(propertyElement);
                properties.Add(kvp.Value);
            }
            return properties;
        }

        private static object ReadPropertySet(XElement element)
        {
            var properties = new Dictionary<string, object>();
            foreach (var propertyElement in element.Elements())
            {
                var kvp = Read(propertyElement);
                properties.Add(kvp.Key, kvp.Value);
            }
            return properties;
        }

        public static void Write(ISchema schema, string entityTypeName, XElement container, IDictionary<string, object> row)
        {
            foreach (var prop in row)
            {
                Write(schema, entityTypeName, container, container, prop);
            }
        }

        public static object ReadEdmBinary(string source)
        {
            return Convert.FromBase64String(source);
        }

        public static object ReadEdmBoolean(string source)
        {
            return source.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public static object ReadEdmByte(string source)
        {
            return Byte.Parse(source);
        }

        public static object ReadEdmDateTime(string source)
        {
            return DateTime.Parse(source, CultureInfo.InvariantCulture);
        }

        public static object ReadEdmDateTimeOffset(string source)
        {
            return DateTimeOffset.Parse(source, CultureInfo.InvariantCulture);
        }

        public static object ReadEdmDecimal(string source)
        {
            return Decimal.Parse(source, CultureInfo.InvariantCulture);
        }

        public static object ReadEdmDouble(string source)
        {
            return Double.Parse(source, CultureInfo.InvariantCulture);
        }

        public static object ReadEdmGuid(string source)
        {
            return new Guid(source);
        }

        public static object ReadEdmInt16(string source)
        {
            return Int16.Parse(source, CultureInfo.InvariantCulture);
        }

        public static object ReadEdmInt32(string source)
        {
            return Int32.Parse(source, CultureInfo.InvariantCulture);
        }

        public static object ReadEdmInt64(string source)
        {
            return Int64.Parse(source, CultureInfo.InvariantCulture);
        }

        public static object ReadEdmSByte(string source)
        {
            return SByte.Parse(source);
        }

        public static object ReadEdmSingle(string source)
        {
            return Single.Parse(source, CultureInfo.InvariantCulture);
        }

        public static object ReadEdmString(string source)
        {
            return source;
        }

        public static object ReadEdmTime(string source)
        {
            return TimeSpan.Parse(source, CultureInfo.InvariantCulture);
        }

        public static string WriteEdmBinary(object source)
        {
            return Convert.ToBase64String((byte[])source);
        }

        public static string WriteEdmBoolean(object source)
        {
            return Convert.ToBoolean(source) ? "true" : "false";
        }

        public static string WriteEdmByte(object source)
        {
            return (Convert.ToByte(source)).ToString();
        }

        public static string WriteEdmDateTime(object source)
        {
            return (Convert.ToDateTime(source)).ToIso8601String();
        }

        public static string WriteEdmDateTimeOffset(object source)
        {
            return ((DateTimeOffset)source).ToIso8601String();
        }

        public static string WriteEdmDouble(object source)
        {
            return (Convert.ToDouble(source)).ToString(CultureInfo.InvariantCulture);
        }

        public static string WriteEdmDecimal(object source)
        {
            return (Convert.ToDecimal(source)).ToString(CultureInfo.InvariantCulture);
        }

        public static string WriteEdmGuid(object source)
        {
            return ((Guid)source).ToString();
        }

        public static string WriteEdmInt16(object source)
        {
            return (Convert.ToInt16(source)).ToString();
        }

        public static string WriteEdmInt32(object source)
        {
            return (Convert.ToInt32(source)).ToString();
        }

        public static string WriteEdmInt64(object source)
        {
            return (Convert.ToInt64(source)).ToString();
        }

        public static string WriteEdmSByte(object source)
        {
            return (Convert.ToSByte(source)).ToString();
        }

        public static string WriteEdmSingle(object source)
        {
            return (Convert.ToSingle(source)).ToString(CultureInfo.InvariantCulture);
        }

        public static string WriteEdmString(object source)
        {
            return source.ToString();
        }

        public static string WriteEdmTime(object source)
        {
            return ((TimeSpan)source).ToIso8601String();
        }

        private static Func<string, object> GetReader(string edmType)
        {
            var func = new Func<string, object>(x => x);

            EdmType.TryParse(edmType).IfGood((et) =>
                {
                    func = Readers[et];
                });

            return func;
        }

        private static void Write(ISchema schema, string typeName, XElement root, XElement container, KeyValuePair<string, object> kvp)
        {
            var element = new XElement(root.GetNamespaceOfPrefix("d") + kvp.Key);

            if (kvp.Value == null)
            {
                element.SetAttributeValue(container.GetNamespaceOfPrefix("m") + "null", "true");
            }
            else
            {
                var property = root == container
                    ? schema
                        .EntityTypes.Single(x => x.Name == typeName)
                        .Properties.Single(x => x.Name == kvp.Key)
                    : schema
                        .ComplexTypes.Single(x => x.Name == typeName)
                        .Properties.Single(x => x.Name == kvp.Key);

                if (property.Type is EdmComplexPropertyType)
                {
                    if (kvp.Value.ToString() == string.Empty)
                    {
                        element.SetAttributeValue(container.GetNamespaceOfPrefix("m") + "null", "true");
                    }
                    else
                    {
                        var edmType = (property.Type as EdmComplexPropertyType).Type;
                        element.SetAttributeValue(root.GetNamespaceOfPrefix("m") + "type", edmType.Name);
                        foreach (var prop in kvp.Value as IDictionary<string, object>)
                        {
                            Write(schema, property.Type.Name, root, element, prop);
                        }
                    }
                }
                else if (property.Type is EdmPrimitivePropertyType)
                {
                    var edmType = (property.Type as EdmPrimitivePropertyType).Type;
                    if (edmType != EdmType.String)
                    {
                        element.SetAttributeValue(root.GetNamespaceOfPrefix("m") + "type", edmType.Name);
                        if (kvp.Value.ToString() == string.Empty)
                        {
                            element.SetAttributeValue(container.GetNamespaceOfPrefix("m") + "null", "true");
                        }
                    }
                    element.SetValue(Writers[edmType](kvp.Value));
                }
                else if (property.Type is EdmEnumPropertyType)
                {
                    var edmType = (property.Type as EdmEnumPropertyType).Type;
                    element.SetAttributeValue(root.GetNamespaceOfPrefix("m") + "type", edmType.Name);
                    if (kvp.Value.ToString() == string.Empty)
                    {
                        element.SetAttributeValue(container.GetNamespaceOfPrefix("m") + "null", "true");
                    }
                    element.SetValue(kvp.Value);
                }
            }

            container.Add(element);
        }
    }
}
