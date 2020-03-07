namespace Simple.OData.Client
{
    /// <summary>
    /// The message of an exception raised after a non successful Http request.
    /// </summary>
    public enum WebRequestExceptionMessage
    {
        /// <summary>
        /// The reason phrase of the http response message.
        /// </summary>
        ReasonPhrase,

        /// <summary>
        /// The content of the http response message.
        /// </summary>
        ResponseContent,

        /// <summary>
        /// Both the reason phrase and conent of the http response message.
        /// </summary>
        Both
    }
}
