using System;
using System.Globalization;

namespace Simple.OData.Client.Extensions
{
    internal static class DateTimeExtensions
    {
        public static string ToIso8601String(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
        }

        public static string ToIso8601String(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
        }

        public static string ToIso8601String(this TimeSpan timeSpan)
        {
            return timeSpan.ToString("HH:mm:ss.fffffffZ");
        }

        public static string ToODataString(this DateTime dateTime, ValueFormatter.FormattingStyle formattingStyle)
        {
            var value = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
            if (formattingStyle == ValueFormatter.FormattingStyle.QueryString)
                return string.Format(@"datetime'{0}'", value);
            else
                return value;
        }

        public static string ToODataString(this DateTimeOffset dateTimeOffset, ValueFormatter.FormattingStyle formattingStyle)
        {
            var value = dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
            if (formattingStyle == ValueFormatter.FormattingStyle.QueryString)
                return string.Format(@"datetimeoffset'{0}'", value);
            else
                return value;
        }

        public static string ToODataString(this TimeSpan timeSpan, ValueFormatter.FormattingStyle formattingStyle)
        {
            var value = timeSpan.ToString();
            if (formattingStyle == ValueFormatter.FormattingStyle.QueryString)
                return string.Format(@"time'{0}'", value);
            else
                return value;
        }
    }
}
