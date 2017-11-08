using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    internal interface IRequestBuilder
    {
        Task<ODataRequest> GetRequestAsync(bool scalarResult, CancellationToken cancellationToken);
        Task<ODataRequest> InsertRequestAsync(bool resultRequired, CancellationToken cancellationToken);
        Task<ODataRequest> UpdateRequestAsync(bool resultRequired, CancellationToken cancellationToken);
        Task<ODataRequest> UpdateRequestAsync(Stream stream, string contentType, bool optimisticConcurrency, CancellationToken cancellationToken);
        Task<ODataRequest> DeleteRequestAsync(CancellationToken cancellationToken);
        Task<ODataRequest> LinkRequestAsync(string linkName, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken);
        Task<ODataRequest> UnlinkRequestAsync(string linkName, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken);
    }

    public interface IRequestBuilder<T>
    {
        Task<IClientWithRequest<T>> FindEntriesAsync();
        Task<IClientWithRequest<T>> FindEntriesAsync(CancellationToken cancellationToken);
        Task<IClientWithRequest<T>> FindEntriesAsync(bool scalarResult);
        Task<IClientWithRequest<T>> FindEntriesAsync(bool scalarResult, CancellationToken cancellationToken);
        Task<IClientWithRequest<T>> FindEntriesAsync(ODataFeedAnnotations annotations);
        Task<IClientWithRequest<T>> FindEntriesAsync(ODataFeedAnnotations annotations, CancellationToken cancellationToken);
        Task<IClientWithRequest<T>> FindEntryAsync();
        Task<IClientWithRequest<T>> FindEntryAsync(CancellationToken cancellationToken);
        Task<IClientWithRequest<T>> InsertEntryAsync();
        Task<IClientWithRequest<T>> InsertEntryAsync(bool resultRequired);
        Task<IClientWithRequest<T>> InsertEntryAsync(CancellationToken cancellationToken);
        Task<IClientWithRequest<T>> InsertEntryAsync(bool resultRequired, CancellationToken cancellationToken);
        Task<IClientWithRequest<T>> UpdateEntryAsync();
        Task<IClientWithRequest<T>> UpdateEntryAsync(bool resultRequired);
        Task<IClientWithRequest<T>> UpdateEntryAsync(CancellationToken cancellationToken);
        Task<IClientWithRequest<T>> UpdateEntryAsync(bool resultRequired, CancellationToken cancellationToken);
        Task<IClientWithRequest<T>> DeleteEntryAsync();
        Task<IClientWithRequest<T>> DeleteEntryAsync(CancellationToken cancellationToken);
    }
}
