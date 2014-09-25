using System.Globalization;

namespace Simple.OData.Client.Extensions
{
    static class DecimalExtensions
    {
        public static string ToODataString(this decimal number)
        {
            var value = number.ToString("F", CultureInfo.InvariantCulture);
            return string.Format(@"{0}M", value);
        }
    }
}