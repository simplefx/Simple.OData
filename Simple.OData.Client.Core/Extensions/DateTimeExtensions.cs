using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.OData.Client
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Formats the DateTime to the ISO 8601 standard, to maximum precision.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>String formatted like "2008-10-01T15:25:05.2852025Z"</returns>
        public static string ToIso8601String(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
        }

        /// <summary>
        /// Formats the DateTimeOffset to the ISO 8601 standard, to maximum precision.
        /// </summary>
        /// <param name="dateTimeOffset">The date time offset.</param>
        /// <returns>String formatted like "2008-10-01T15:25:05.2852025Z"</returns>
        public static string ToIso8601String(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
        }

        /// <summary>
        /// Formats the TimeSpan to the ISO 8601 standard, to maximum precision.
        /// </summary>
        /// <param name="timeSpan">The timespan.</param>
        /// <returns>String formatted like "2008-10-01T15:25:05.2852025Z"</returns>
        public static string ToIso8601String(this TimeSpan timeSpan)
        {
            return timeSpan.ToString("HH:mm:ss.fffffffZ");
        }
    }
}
