using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    /// <summary>
    /// Provides access to OData media stream operations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IMediaClient
    {
        /// <summary>
        /// Retrieves a media stream by executing OData GET request.
        /// </summary>
        /// <returns>The media stream.</returns>
        Task<Stream> GetStreamAsync();
        /// <summary>
        /// Retrieves a media stream by executing OData GET request.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The media stream.</returns>
        Task<Stream> GetStreamAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Retrieves a media stream as byte array by executing OData GET request.
        /// </summary>
        /// <returns>The media stream converted to byte array.</returns>
        Task<byte[]> GetStreamAsArrayAsync();
        /// <summary>
        /// Retrieves a media stream as byte array by executing OData GET request.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The media stream converted to byte array.</returns>
        Task<byte[]> GetStreamAsArrayAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Retrieves a media stream as string by executing OData GET request.
        /// </summary>
        /// <returns>The media stream converted to string.</returns>
        Task<string> GetStreamAsStringAsync();
        /// <summary>
        /// Retrieves a media stream as string by executing OData GET request.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The media stream converted to string.</returns>
        Task<string> GetStreamAsStringAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Assigns a media stream by executing OData PUT request.
        /// </summary>
        /// <param name="stream">The media stream.</param>
        /// <param name="contentType">The media content type.</param>
        /// <param name="optimisticConcurrency">if set to <c>true</c> sets the resource ETag in the updated request header.</param>
        /// <returns>Task instance.</returns>
        Task SetStreamAsync(Stream stream, string contentType, bool optimisticConcurrency);
        /// <summary>
        /// Assigns a media stream by executing OData PUT request.
        /// </summary>
        /// <param name="stream">The media stream.</param>
        /// <param name="contentType">The media content type.</param>
        /// <param name="optimisticConcurrency">if set to <c>true</c> sets the resource ETag in the updated request header.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task instance.</returns>
        Task SetStreamAsync(Stream stream, string contentType, bool optimisticConcurrency, CancellationToken cancellationToken);
        /// <summary>
        /// Assigns a media stream by executing OData PUT request.
        /// </summary>
        /// <param name="streamContent">The media stream content represented as byte array.</param>
        /// <param name="contentType">The media content type.</param>
        /// <param name="optimisticConcurrency">if set to <c>true</c> sets the resource ETag in the updated request header.</param>
        /// <returns>Task instance.</returns>
        Task SetStreamAsync(byte[] streamContent, string contentType, bool optimisticConcurrency);
        /// <summary>
        /// Assigns a media stream by executing OData PUT request.
        /// </summary>
        /// <param name="streamContent">The media stream content represented as byte array.</param>
        /// <param name="contentType">The media content type.</param>
        /// <param name="optimisticConcurrency">if set to <c>true</c> sets the resource ETag in the updated request header.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task instance.</returns>
        Task SetStreamAsync(byte[] streamContent, string contentType, bool optimisticConcurrency, CancellationToken cancellationToken);
        /// <summary>
        /// Assigns a media stream by executing OData PUT request.
        /// </summary>
        /// <param name="streamContent">The media stream content represented as string.</param>
        /// <param name="optimisticConcurrency">if set to <c>true</c> sets the resource ETag in the updated request header.</param>
        /// <returns>Task instance.</returns>
        Task SetStreamAsync(string streamContent, bool optimisticConcurrency);
        /// <summary>
        /// Assigns a media stream by executing OData PUT request.
        /// </summary>
        /// <param name="streamContent">The media stream content represented as string.</param>
        /// <param name="optimisticConcurrency">if set to <c>true</c> sets the resource ETag in the updated request header.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task instance.</returns>
        Task SetStreamAsync(string streamContent, bool optimisticConcurrency, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the OData command text.
        /// </summary>
        /// <returns>The command text.</returns>
        Task<string> GetCommandTextAsync();
        /// <summary>
        /// Gets the OData command text.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The command text.</returns>
        Task<string> GetCommandTextAsync(CancellationToken cancellationToken);
    }
}
