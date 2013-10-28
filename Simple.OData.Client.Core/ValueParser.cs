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

        private object ParseValue(string value, EdmPropertyType propertyType)
        {
            return value == "null" ? null
                : propertyType.Name == EdmType.Boolean.Name ? bool.Parse(value)
                : propertyType.Name == EdmType.Byte.Name ? byte.Parse(value)
                : propertyType.Name == EdmType.SByte.Name ? sbyte.Parse(value)
                : propertyType.Name == EdmType.DateTime.Name ? DateTime.Parse(value, CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.DateTimeOffset.Name ? DateTimeOffset.Parse(value, CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Int16.Name ? short.Parse(value, CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Int32.Name ? int.Parse(value, CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Int64.Name ? long.Parse(value, CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Single.Name ? float.Parse(value, CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Float.Name ? float.Parse(value, CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Double.Name ? double.Parse(value, CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Decimal.Name ? decimal.Parse(value, CultureInfo.InvariantCulture)
                : propertyType.Name == EdmType.Guid.Name ? Guid.Parse(value)
                : propertyType.Name == EdmType.String.Name ? Uri.UnescapeDataString(value.Substring(1, value.Length - 2))
                : propertyType.Name == EdmType.Time.Name ? TimeSpan.Parse(value, CultureInfo.InvariantCulture)
                : (object)value;
        }
    }
}
