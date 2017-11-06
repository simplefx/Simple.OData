using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    internal class ClientRequestBuilder<T> : IClientRequestBuilder<T>
        where T : class
    {
        private readonly Session _session;
        private readonly FluentCommand _command;

        public ClientRequestBuilder(ODataClient client, Session session, FluentCommand command = null)
        {
            _session = session;
            _command = command;
        }

        public Task<IClientWithRequest<T>> FindEntryAsync()
        {
            return FindEntryAsync(CancellationToken.None);
        }

        public async Task<IClientWithRequest<T>> FindEntryAsync(CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await _command.GetCommandTextAsync(cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var request = await _session.Adapter.GetRequestWriter(null).CreateGetRequestAsync(commandText, false).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return new ClientWithRequest<T>(request, _session);
        }
    }
}
