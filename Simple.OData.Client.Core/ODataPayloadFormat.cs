namespace Simple.OData.Client
{
    /// <summary>
    /// The format of the messages exchanged with OData service.
    /// </summary>
    public enum ODataPayloadFormat
    {
        /// <summary>
        /// The message format is not specified and will be determined by the OData service.
        /// </summary>
        Unspecified,

        /// <summary>
        /// OData message content will be formatted as Atom
        /// </summary>
        Atom,

        /// <summary>
        /// OData message content will be formatted as JSON
        /// </summary>
        Json,
    }
}