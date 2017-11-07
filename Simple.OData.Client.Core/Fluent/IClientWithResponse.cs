using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IClientWithResponse<T> : IDisposable
    {
        Task<IEnumerable<T>> ReadAsCollectionAsync();
        Task<IEnumerable<T>> ReadAsCollectionAsync(CancellationToken cancellationToken);

        Task<T> ReadAsSingleAsync();
        Task<T> ReadAsSingleAsync(CancellationToken cancellationToken);

        Task<Stream> GetResponseStreamAsync();
        Task<Stream> GetResponseStreamAsync(CancellationToken cancellationToken);
    }
}
