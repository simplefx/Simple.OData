using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    internal class ValueFormatter
    {
        public string FormatValue(object value)
        {
            return value == null ? "null"
                : value is string ? string.Format("'{0}'", value)
                : value is DateTime ? ((DateTime)value).ToODataString()
                : value is DateTimeOffset ? ((DateTimeOffset)value).ToODataString()
                : value is TimeSpan ? ((TimeSpan)value).ToODataString()
                : value is Guid ? ((Guid)value).ToODataString()
                : value is bool ? value.ToString().ToLower()
                : value is long ? ((long)value).ToODataString()
                : value is ulong ? ((ulong)value).ToODataString()
                : value is float ? ((float)value).ToString(CultureInfo.InvariantCulture)
                : value is double ? ((double)value).ToString(CultureInfo.InvariantCulture)
                : value is decimal ? ((decimal)value).ToODataString()
                : value.ToString();
        }
    }
}
