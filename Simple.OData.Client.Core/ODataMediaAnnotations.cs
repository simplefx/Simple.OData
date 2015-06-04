using System;

namespace Simple.OData.Client
{

    /// <summary>
    /// Contains additional information about OData media resource
    /// </summary>
    public class ODataMediaAnnotations
    {
        /// <summary>
        /// The media resource content type.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The link that can be used to read the media resource.
        /// </summary>
        public Uri ReadLink { get; set; }

        /// <summary>
        /// The link can be used to edit the media resource.
        /// </summary>
        public Uri EditLink { get; set; }

        /// <summary>
        /// The media resource ETag.
        /// </summary>
        public string ETag { get; set; }
    }
}