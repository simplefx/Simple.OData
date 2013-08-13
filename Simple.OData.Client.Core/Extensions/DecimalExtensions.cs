using System.Globalization;

namespace Simple.OData.Client.Extensions
{
    internal static class DecimalExtensions
    {
        public static string ToODataString(this decimal number, ValueFormatter.FormattingStyle formattingStyle)
        {
            var value = number.ToString("F", CultureInfo.InvariantCulture);
            return string.Format(@"{0}M", value);
        }
    }
}