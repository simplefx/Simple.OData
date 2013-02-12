using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Simple.OData.Client
{
    internal class ValueFormatter
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
            return FormatValue(value, FormattingStyle.Content, null);
        }

        public string FormatQueryStringValue(object value)
        {
            return FormatValue(value, FormattingStyle.QueryString, null);
        }

        public string FormatExpressionValue(object value, ExpressionContext context)
        {
            return FormatValue(value, FormattingStyle.Content, context);
        }

        private string FormatValue(object value, FormattingStyle formattingStyle, ExpressionContext context)
        {
            return value == null ? "null"
                : value is FilterExpression ? (value as FilterExpression).Format(context)
                : value is string ? string.Format("'{0}'", value)
                : value is DateTime ? ((DateTime)value).ToIso8601String()
                : value is bool ? value.ToString().ToLower()
                : (value is long || value is ulong) ? value + (formattingStyle == FormattingStyle.Content ? "L" : string.Empty)
                : value is float ? ((float)value).ToString(CultureInfo.InvariantCulture)
                : value is double ? ((double)value).ToString(CultureInfo.InvariantCulture)
                : value is decimal ? ((decimal)value).ToString(CultureInfo.InvariantCulture)
                : value.ToString();
        }
    }
}
