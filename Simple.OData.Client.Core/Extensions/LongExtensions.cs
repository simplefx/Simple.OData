using System.Globalization;

namespace Simple.OData.Client.Extensions
{
    internal static class LongExtensions
    {
        public static string ToODataString(this long number, ValueFormatter.FormattingStyle formattingStyle)
        {
            var value = number.ToString(CultureInfo.InvariantCulture);
            return string.Format(@"{0}L", value);
        }
    }
}