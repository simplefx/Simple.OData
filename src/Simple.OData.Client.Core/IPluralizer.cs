namespace Simple.OData.Client
{
    /// <summary>
    /// Provides pluralization and singularization of words when resolving names of resources and properties
    /// </summary>
    public interface IPluralizer
    {
        /// <summary>
        /// Pluralizes the specified word.
        /// </summary>
        /// <param name="word">The word to pluralize.</param>
        /// <returns></returns>
        string Pluralize(string word);

        /// <summary>
        /// Singularizes the specified word.
        /// </summary>
        /// <param name="word">The word to singularize.</param>
        /// <returns></returns>
        string Singularize(string word);
    }
}
