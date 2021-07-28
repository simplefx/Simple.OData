namespace Simple.OData.Client
{
    /// <summary>
    /// Indicates the format of Request-URI in each sub request in the batch operation.
    /// </summary>
    /// <remarks>
    /// This must be kept aligned with https://github.com/OData/odata.net/blob/master/src/Microsoft.OData.Core/Batch/ODataBatchPayloadUriOptions.cs
    /// </remarks>
    public enum BatchPayloadUriOption
    {
        /// <summary>
        /// Absolute URI with schema, host, port, and absolute resource path.
        /// </summary>
        /// Example:
        /// GET https://host:1234/path/service/People(1) HTTP/1.1
        AbsoluteUri,

        /// <summary>
        /// Absolute resource path and separate Host header.
        /// </summary>
        /// Example:
        /// GET /path/service/People(1) HTTP/1.1
        /// Host: myserver.mydomain.org:1234
        AbsoluteUriUsingHostHeader,

        /// <summary>
        /// Resource path relative to the batch request URI.
        /// </summary>
        /// Example:
        /// GET People(1) HTTP/1.1
        RelativeUri
    }
}