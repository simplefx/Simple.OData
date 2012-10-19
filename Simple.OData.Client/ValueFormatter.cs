using System;
using System.Collections.Generic;
using System.Linq;
using Simple.NExtLib;

namespace Simple.OData.Client
{
    public class ValueFormatter
    {
        enum FormattingStyle
        {
            QueryString,
            Content
        };

        public ValueFormatter()
        {
        }

        public string Format(IDictionary<string, object> keyValues, string separator = ",")
        {
            return string.Join(separator, keyValues.Select(x => string.Format("{0}={1}", x.Key, FormatContentValue(x.Value))));
        }

        public string Format(IEnumerable<object> keyValues, string separator = ",")
        {
            return string.Join(separator, keyValues.Select(FormatContentValue));
        }

        public string FormatContentValue(object value)
        {
            return FormatValue(value, FormattingStyle.Content);
        }

        public string FormatQueryStringValue(object value)
        {
            return FormatValue(value, FormattingStyle.QueryString);
        }

        private string FormatValue(object value, FormattingStyle formattingStyle)
        {
            return value == null ? "null"
                : value is string ? string.Format("'{0}'", value)
                : value is DateTime ? ((DateTime)value).ToIso8601String()
                : value is bool ? ((bool)value) ? "true" : "false"
                : (formattingStyle == FormattingStyle.Content && (value is long || value is ulong)) ? value.ToString() + "L"
                : value.ToString();
        }
    }
}
