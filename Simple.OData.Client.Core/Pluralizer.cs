using System;

namespace Simple.OData.Client
{
    /// <summary>
    /// Provides pluralization and singularization of words when resolving names of resources and properties
    /// </summary>
    public class Pluralizer : IPluralizer
    {
        private readonly Func<string, string> _pluralize;
        private readonly Func<string, string> _singularize;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pluralizer"/> class.
        /// </summary>
        /// <param name="pluralize">The Pluralize function delegate.</param>
        /// <param name="singularize">The Singularize function delegate.</param>
        public Pluralizer(Func<string, string> pluralize, Func<string, string> singularize)
        {
            _pluralize = pluralize;
            _singularize = singularize;
        }

        /// <summary>
        /// Pluralizes the specified word.
        /// </summary>
        /// <param name="word">The word to pluralize.</param>
        /// <returns></returns>
        public string Pluralize(string word)
        {
            return _pluralize(word);
        }

        /// <summary>
        /// Singularizes the specified word.
        /// </summary>
        /// <param name="word">The word to singularize.</param>
        /// <returns></returns>
        public string Singularize(string word)
        {
            return _singularize(word);
        }
    }
}
