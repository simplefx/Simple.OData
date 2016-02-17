using System;

namespace Simple.OData.Client.Tests
{
    public interface IFormatSettings
    {
        int ODataVersion { get; }
        string LongNumberSuffix { get; }
        string DoubleNumberSuffix { get; }
        string DecimalNumberSuffix { get; }
        string TimeSpanPrefix { get; }
        string GetDateTimeOffsetFormat(string text, bool escapeDataString = false);
        string GetGuidFormat(string text, bool escapeDataString = false);
        string GetEnumFormat(object value, Type enumType, string ns, bool prefixFree = false, bool escapeDataString = false);
        string GetContainsFormat(string item, string text, bool escapeDataString = false);
    }

    class ODataV3Format : IFormatSettings
    {
        public int ODataVersion { get { return 3; } }
        public string LongNumberSuffix { get { return "L"; } }
        public string DoubleNumberSuffix { get { return "D"; } }
        public string DecimalNumberSuffix { get { return "M"; } }
        public string TimeSpanPrefix { get { return "time"; } }

        public string GetDateTimeOffsetFormat(string text, bool escapeDataString = false)
        {
            var result = string.Format("datetimeoffset'{0}'", text);
            if (escapeDataString)
                result = Uri.EscapeDataString(result);
            return result;
        }

        public string GetGuidFormat(string text, bool escapeDataString = false)
        {
            var result = string.Format("guid'{0}'", text);
            if (escapeDataString)
                result = Uri.EscapeDataString(result);
            return result;
        }

        public string GetEnumFormat(object value, Type enumType, string ns, bool prefixFree = false, bool escapeDataString = false)
        {
            return Convert.ToInt32(value).ToString();
        }

        public string GetContainsFormat(string item, string text, bool escapeDataString = false)
        {
            var result = string.Format("substringof('{0}',{1})", text, item);
            if (escapeDataString)
                result = Uri.EscapeDataString(result);
            return result;
        }
    }

    class ODataV4Format : IFormatSettings
    {
        public ODataV4Format(bool escapeUri = false)
        {
        }

        public int ODataVersion { get { return 4; } }
        public string LongNumberSuffix { get { return string.Empty; } }
        public string DoubleNumberSuffix { get { return string.Empty; } }
        public string DecimalNumberSuffix { get { return string.Empty; } }
        public string TimeSpanPrefix { get { return "duration"; } }

        public string GetDateTimeOffsetFormat(string text, bool escapeDataString = false)
        {
            return escapeDataString ? Uri.EscapeDataString(text) : text;
        }

        public string GetGuidFormat(string text, bool escapeDataString = false)
        {
            return escapeDataString ? Uri.EscapeDataString(text) : text;
        }

        public string GetEnumFormat(object value, Type enumType, string ns, bool prefixFree = false, bool escapeDataString = false)
        {
            var result = prefixFree
                ? string.Format("'{0}'", Enum.ToObject(enumType, value))
                : string.Format("{0}.{1}'{2}'", ns, enumType.Name, Enum.ToObject(enumType, value));
            if (escapeDataString)
                result = Uri.EscapeDataString(result);
            return result;
        }

        public string GetContainsFormat(string item, string text, bool escapeDataString = false)
        {
            var result = string.Format("contains({0},'{1}')", item, text);
            if (escapeDataString)
                result = Uri.EscapeDataString(result);
            return result;
        }
    }
}