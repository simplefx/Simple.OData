using System;
using System.Linq;

namespace Simple.OData.Client.Extensions
{
    static class StringExtensions
    {
        private static IPluralizer _pluralizer = new SimplePluralizer();

        internal static void SetPluralizer(IPluralizer pluralizer)
        {
            _pluralizer = pluralizer;
        }

        public static string Pluralize(this string str)
        {
            return _pluralizer != null ? _pluralizer.Pluralize(str) : str;
        }

        public static string Singularize(this string str)
        {
            return _pluralizer != null ? _pluralizer.Singularize(str) : str;
        }

        public static bool IsAllUpperCase(this string str)
        {
            return !str.Cast<char>().Any(char.IsLower);
        }

        public static string NullIfWhitespace(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? null : str;
        }

        public static string OrDefault(this string str, string defaultValue)
        {
            return str ?? defaultValue;
        }

        public static string EnsureStartsWith(this string source, string value)
        {
            return (source == null || source.StartsWith(value)) ? source : value + source;
        }
    }
}
