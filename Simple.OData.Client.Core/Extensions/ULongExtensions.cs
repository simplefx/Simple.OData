using System.Globalization;

namespace Simple.OData.Client.Extensions
{
    internal static class ULongExtensions
    {
        public static string ToODataString(this ulong number, ValueFormatter.FormattingStyle formattingStyle)
        {
            var value = number.ToString(CultureInfo.InvariantCulture);
            if (formattingStyle == ValueFormatter.FormattingStyle.Content)
                return string.Format(@"{0}L", value);
            else
                return value;
        }
    }
}