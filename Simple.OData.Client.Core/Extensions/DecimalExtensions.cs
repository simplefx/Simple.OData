using System.Globalization;

namespace Simple.OData.Client.Extensions
{
    internal static class DecimalExtensions
    {
        public static string ToODataString(this decimal number, ValueFormatter.FormattingStyle formattingStyle)
        {
            if (formattingStyle == ValueFormatter.FormattingStyle.QueryString)
            {
                var value = number.ToString("F", CultureInfo.InvariantCulture);
                return string.Format(@"{0}M", value);
            }
            else
            {
                return number.ToString(CultureInfo.InvariantCulture); ;
            }
        }
    }
}