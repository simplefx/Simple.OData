namespace Simple.OData.Client
{
    /// <summary>
    /// The format of OData function arguments.
    /// </summary>
    public enum FunctionFormat
    {
        /// <summary>
        /// The function arguments will be formatted as a query string
        /// </summary>
        Query,

        /// <summary>
        /// The function arguments will be formatted as a a key-value list
        /// </summary>
        Key,
    }
}