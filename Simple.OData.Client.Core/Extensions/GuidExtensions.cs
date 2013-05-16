using System;

namespace Simple.OData.Client.Extensions
{
    internal static class GuidExtensions
    {
        public static string ToODataString(this Guid guid, ValueFormatter.FormattingStyle formattingStyle)
        {
            var value = guid.ToString();
            if (formattingStyle == ValueFormatter.FormattingStyle.QueryString)
                return string.Format(@"guid'{0}'", value);
            else
                return value;
        }
    }
}