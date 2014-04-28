using System;
using System.Linq;

namespace Simple.OData.Client.Extensions
{
    static class StringExtensions
    {
        public static bool IsPlural(this string str)
        {
            return _pluralizer.IsPlural(str);
        }

        public static string Pluralize(this string str)
        {
            return _pluralizer.Pluralize(str);
        }

        public static string Singularize(this string str)
        {
            return _pluralizer.Singularize(str);
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

        private static IPluralizer _pluralizer = new SimplePluralizer();

        internal static void SetPluralizer(IPluralizer pluralizer)
        {
            _pluralizer = pluralizer ?? new SimplePluralizer();
        }

        public static string EnsureStartsWith(this string source, string value)
        {
            return (source == null || source.StartsWith(value)) ? source : value + source;
        }
    }

    class SimplePluralizer : IPluralizer
    {
        public bool IsSingular(string word)
        {
            return !IsPlural(word);
        }

        public bool IsPlural(string word)
        {
            return word.EndsWith("s", StringComparison.OrdinalIgnoreCase);
        }

        public string Pluralize(string word)
        {
            if (word.EndsWith("y", StringComparison.OrdinalIgnoreCase))
                word = word.Substring(0, word.Length-1) + (word.IsAllUpperCase() ? "IE" : "ie");
            return string.Concat(word, word.IsAllUpperCase() ? "S" : "s");
        }

        public string Singularize(string word)
        {
            if (word.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
                return word.Substring(0, word.Length - 3) + (word.IsAllUpperCase() ? "Y" : "y");
            if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                return word.Substring(0, word.Length - 1);
            else
                return word;
        }

    }
}
