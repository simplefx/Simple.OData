using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IClientWithResponse<T> : IDisposable
    {
        HttpResponseMessage ResponseMessage { get; }

        Task<IEnumerable<T>> ReadAsCollectionAsync();
        Task<IEnumerable<T>> ReadAsCollectionAsync(CancellationToken cancellationToken);
        Task<IEnumerable<T>> ReadAsCollectionAsync(ODataFeedAnnotations annotations);
        Task<IEnumerable<T>> ReadAsCollectionAsync(ODataFeedAnnotations annotations, CancellationToken cancellationToken);

        Task<T> ReadAsSingleAsync();
        Task<T> ReadAsSingleAsync(CancellationToken cancellationToken);

        Task<U> ReadAsScalarAsync<U>();
        Task<U> ReadAsScalarAsync<U>(CancellationToken cancellationToken);

        Task<Stream> GetResponseStreamAsync();
        Task<Stream> GetResponseStreamAsync(CancellationToken cancellationToken);
    }
}
