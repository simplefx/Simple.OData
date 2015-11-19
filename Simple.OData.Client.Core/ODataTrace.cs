namespace Simple.OData.Client
{
    /// <summary>
    /// Specifies types of information to be written to trace messages.
    /// </summary>
    public enum ODataTrace
    {
        /// <summary>
        /// No trace information is written to trace messages.
        /// </summary>
        None,

        /// <summary>
        /// Trace the contents of HTTP requests.
        /// </summary>
        RequestContent = 0x1,

        /// <summary>
        /// Trace the contents of HTTP responses.
        /// </summary>
        ResponseContent = 0x2,

        /// <summary>
        /// Trace all internal activities.
        /// </summary>
        All = 0xFF,
    }
}