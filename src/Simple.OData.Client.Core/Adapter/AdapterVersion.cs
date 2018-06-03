using System;

namespace Simple.OData.Client
{
    /// <summary>
    /// The OData protocol version of the OData adapter.
    /// </summary>
    [Flags]
    public enum AdapterVersion
    {
        /// <summary>
        /// OData protocol version 3
        /// </summary>
        V3 = 0x01,

        /// <summary>
        /// OData protocol version 4
        /// </summary>
        V4 = 0x10,

        /// <summary>
        /// Default OData protocol version
        /// </summary>
        Default = V4,
        
        /// <summary>
        /// Any OData protocol version
        /// </summary>
        Any = V3 | V4,
    }
}