using System;

namespace Simple.OData.Client
{
    public class Pluralizer : IPluralizer
    {
        private Func<string, bool> _isPlural;
        private Func<string, bool> _isSingular;
        private Func<string, string> _pluralize;
        private Func<string, string> _singularize;

        public Pluralizer(Func<string, bool> isPlural, Func<string, bool> isSingular, Func<string, string> pluralize, Func<string, string> singularize)
        {
            _isPlural = isPlural;
            _isSingular = isSingular;
            _pluralize = pluralize;
            _singularize = singularize;
        }

        public bool IsPlural(string word)
        {
            return _isPlural(word);
        }

        public bool IsSingular(string word)
        {
            return _isSingular(word);
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
