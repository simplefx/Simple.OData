using System;

namespace Simple.OData.Client
{
    public class Pluralizer : IPluralizer
    {
        private readonly Func<string, string> _pluralize;
        private readonly Func<string, string> _singularize;

        public Pluralizer(Func<string, string> pluralize, Func<string, string> singularize)
        {
            _pluralize = pluralize;
            _singularize = singularize;
        }

        public string Pluralize(string word)
        {
            return _pluralize(word);
        }

        public string Singularize(string word)
        {
            return _singularize(word);
        }
    }
}
