using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IClientWithRequest<T>
    {
        ODataRequest GetRequest();

        Task<IClientWithResponse<T>> RunAsync();
        Task<IClientWithResponse<T>> RunAsync(CancellationToken cancellationToken);
    }
}
