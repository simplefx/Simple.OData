using System;
using System.Net;
using System.Net.Http;

namespace Simple.OData.Client
{
    /// <summary>
    /// Provide access to session-specific details.
    /// </summary>
    public interface ISession : IDisposable
    {
        /// <summary>
        /// Gets OData client configuration settings.
        /// </summary>
        ODataClientSettings Settings { get; }

        /// <summary>
        /// Gets OData client adapter.
        /// </summary>
        IODataAdapter Adapter { get; }

        /// <summary>
        /// Gets OData service metadata.
        /// </summary>
        IMetadata Metadata { get; }

        /// <summary>
        /// Gets word pluralizer.
        /// </summary>
        IPluralizer Pluralizer { get; }

        /// <summary>
        /// Writes a trace message.
        /// </summary>
        /// <param name="message">Trace message format string.</param>
        /// <param name="messageParams">Trace message parameters.</param>
        void Trace(string message, params object[] messageParams);

        HttpClient GetHttpClient();
    }
}