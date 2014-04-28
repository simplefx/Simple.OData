using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    internal class ValueParser
    {
        private readonly Table _table;

        public ValueParser(Table table)
        {
            _table = table;
        }

        public IDictionary<string, object> Parse(string keyValues)
        {
            if (_table.GetKeyNames().Count == 1)
            {
                var columnName = _table.GetKeyNames()[0];
                return new Dictionary<string, object>() { { columnName, ParseValue(keyValues, _table.FindColumn(columnName).PropertyType) } };
            }
            else
            {
                var dict = new Dictionary<string, object>();
                var kvs = keyValues.Split(',');
                foreach (var kv in kvs)
                {
                    var pair = kv.Split('=');
                    var columnName = pair.First();
                    dict.Add(columnName, ParseValue(pair.Last(), _table.FindColumn(columnName).PropertyType));
                }
                return dict;
            }
        }

        public object ParseValue(string value, EdmPropertyType propertyType)
        {
            return value == "null" ? null
                : propertyType.Name == EdmType.Binary.Name ? ParseBinary(RemoveLiteral(RemoveLiteral(value, "binary"), "X"))
                : propertyType.Name == EdmType.Boolean.Name ? bool.Parse(value)
                : propertyType.Name == EdmType.Byte.Name ? byte.Parse(value)
                : propertyType.Name == EdmType.SByte.Name ? sbyte.Parse(value)
                : propertyType.Name == EdmType.DateTime.Name ? DateTime.Parse(RemoveLiteral(value, "datetime"), CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.DateTimeOffset.Name ? DateTimeOffset.Parse(RemoveLiteral(value, "datetimeoffset"), CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Int16.Name ? short.Parse(value, CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Int32.Name ? int.Parse(value, CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Int64.Name ? long.Parse(value.TrimEnd('L'), CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Single.Name ? float.Parse(value.TrimEnd('f'), CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Float.Name ? float.Parse(value, CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Double.Name ? double.Parse(value.TrimEnd('d'), CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Decimal.Name ? decimal.Parse(value.TrimEnd('M', 'm'), CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Guid.Name ? Guid.Parse(RemoveLiteral(value, "guid"))
                : propertyType.Name == EdmType.String.Name ? Uri.UnescapeDataString(value.Substring(1, value.Length - 2))
                : propertyType.Name == EdmType.Time.Name ? TimeSpan.Parse(RemoveLiteral(value, "time"), CultureInfo.InvariantCulture)
                : (object)value;
        }

        private string RemoveLiteral(string value, string literal)
        {
            value = value.Trim();
            if (value.ToLower().StartsWith(literal.ToLower()))
            {
                return value.Substring(literal.Length).Replace("'", "");
            }
            else
            {
                return value;
            }
        }

        private byte[] ParseBinary(string value)
        {
            var arrayParts = new List<byte>();
            while (value.Length > 0)
            {
                var partLength = Math.Min(value.Length, 2);
                var part = value.Substring(0, partLength);
                var byteValue = byte.Parse(part, NumberStyles.AllowHexSpecifier);
                arrayParts.Add(byteValue);
                value = value.Substring(partLength);
            }
            return arrayParts.ToArray();
        }
    }
}
