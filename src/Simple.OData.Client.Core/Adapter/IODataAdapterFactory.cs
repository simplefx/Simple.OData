using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    /// <summary>
    /// IODataAdapterFactory interface contains collection of methods to create OData adapters.
    /// It can be used as a customization point to create adapters with custom behavior.
    /// </summary>
    public interface IODataAdapterFactory
    {
        /// <summary>
        /// Creates an instance of IODataModelAdapter from an HTTP response with schema information.
        /// </summary>
        /// <param name="response">HTTP response message with schema information</param>
        /// <param name="typeCache">Service type cache</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        Task<IODataModelAdapter> CreateModelAdapterAsync(HttpResponseMessage response, ITypeCache typeCache);

        /// <summary>
        /// Creates an instance of IODataModelAdapter from a metadata string.
        /// </summary>
        /// <param name="metadataString">Service metadata</param>
        /// <param name="typeCache">Service type cache</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        IODataModelAdapter CreateModelAdapter(string metadataString, ITypeCache typeCache);

        /// <summary>
        /// Creates an instance of OData adapter loader from a metadata string.
        /// </summary>
        /// <param name="metadataString">Service metadata</param>
        /// <param name="typeCache">Service type cache</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        Func<ISession, IODataAdapter> CreateAdapterLoader(string metadataString, ITypeCache typeCache);
    }
}