using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    internal class RequestBuilder : IRequestBuilder
    {
        private readonly FluentCommand _command;
        private readonly Session _session;
        private readonly Lazy<IBatchWriter> _lazyBatchWriter;

        public RequestBuilder(FluentCommand command, Session session, Lazy<IBatchWriter> batchWriter)
        {
            _session = session;
            _command = command;
            _lazyBatchWriter = batchWriter;
        }

        public async Task<ODataRequest> GetRequestAsync(bool scalarResult, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var commandText = await _command.GetCommandTextAsync(cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await _session.Adapter.GetRequestWriter(_lazyBatchWriter).CreateGetRequestAsync(commandText, scalarResult).ConfigureAwait(false);
        }

        public async Task<ODataRequest> InsertRequestAsync(bool resultRequired, CancellationToken cancellationToken)
        {
            await _session.ResolveAdapterAsync(cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var entryData = _command.CommandData;
            var commandText = await _command.GetCommandTextAsync(cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateInsertRequestAsync(_command.QualifiedEntityCollectionName, commandText, entryData, resultRequired).ConfigureAwait(false);
        }

        public async Task<ODataRequest> UpdateRequestAsync(bool resultRequired, CancellationToken cancellationToken)
        {
            AssertHasKey(_command);

            await _session.ResolveAdapterAsync(cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _command.QualifiedEntityCollectionName;
            var entryKey = _command.HasKey ? _command.KeyValues : _command.FilterAsKey;
            var entryData = _command.CommandData;
            var entryIdent = await FormatEntryKeyAsync(_command, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await _session.Adapter.GetRequestWriter(_lazyBatchWriter).CreateUpdateRequestAsync(collectionName, entryIdent, entryKey, entryData, resultRequired).ConfigureAwait(false);
        }

        public async Task<ODataRequest> DeleteRequestAsync(CancellationToken cancellationToken)
        {
            AssertHasKey(_command);

            await _session.ResolveAdapterAsync(cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _command.QualifiedEntityCollectionName;
            var entryIdent = await FormatEntryKeyAsync(_command, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await _session.Adapter.GetRequestWriter(_lazyBatchWriter).CreateDeleteRequestAsync(collectionName, entryIdent).ConfigureAwait(false);
        }

        public async Task<ODataRequest> LinkRequestAsync(string linkName, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken)
        {
            AssertHasKey(_command);

            await _session.ResolveAdapterAsync(cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _command.QualifiedEntityCollectionName;
            var entryKey = _command.HasKey ? _command.KeyValues : _command.FilterAsKey;

            var entryIdent = await FormatEntryKeyAsync(collectionName, entryKey, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var linkedCollection = _session.Metadata.GetNavigationPropertyPartnerTypeName(collectionName, linkName);
            var linkIdent = await FormatEntryKeyAsync(linkedCollection, linkedEntryKey, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            return await _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateLinkRequestAsync(collectionName, linkName, entryIdent, linkIdent).ConfigureAwait(false);
        }

        public async Task<ODataRequest> UnlinkRequestAsync(string linkName, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken)
        {
            AssertHasKey(_command);

            await _session.ResolveAdapterAsync(cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            var collectionName = _command.QualifiedEntityCollectionName;
            var entryKey = _command.HasKey ? _command.KeyValues : _command.FilterAsKey;

            var entryIdent = await FormatEntryKeyAsync(collectionName, entryKey, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();

            string linkIdent = null;
            if (linkedEntryKey != null)
            {
                var linkedCollection = _session.Metadata.GetNavigationPropertyPartnerTypeName(collectionName, linkName);
                linkIdent = await FormatEntryKeyAsync(linkedCollection, linkedEntryKey, cancellationToken).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested) cancellationToken.ThrowIfCancellationRequested();
            }

            return await _session.Adapter.GetRequestWriter(_lazyBatchWriter)
                .CreateUnlinkRequestAsync(collectionName, linkName, entryIdent, linkIdent).ConfigureAwait(false);
        }

        private async Task<string> FormatEntryKeyAsync(FluentCommand command, CancellationToken cancellationToken)
        {
            var entryIdent = command.HasKey
                ? await command.GetCommandTextAsync(cancellationToken).ConfigureAwait(false) 
                : await (new FluentCommand(command).Key(command.FilterAsKey).GetCommandTextAsync(cancellationToken)).ConfigureAwait(false);

            return entryIdent;
        }

        private async Task<string> FormatEntryKeyAsync(string collection, IDictionary<string, object> entryKey, CancellationToken cancellationToken)
        {
            var client = new BoundClient<IDictionary<string, object>>(new ODataClient(_session.Settings), _session);
            var entryIdent = await client
                .For(collection)
                .Key(entryKey)
                .GetCommandTextAsync(cancellationToken).ConfigureAwait(false);

            return entryIdent;
        }

        private void AssertHasKey(FluentCommand command)
        {
            if (!command.HasKey && command.FilterAsKey == null)
                throw new InvalidOperationException("No entry key specified.");
        }
    }

    internal class RequestBuilder<T> : IRequestBuilder<T>
        where T : class
    {
        private readonly RequestBuilder _builder;
        protected readonly Session _session;

        public RequestBuilder(FluentCommand command, Session session, Lazy<IBatchWriter> batchWriter)
        {
            _builder = new RequestBuilder(command, session, batchWriter);
        }

        public Task<IClientWithRequest<T>> FindEntriesAsync()
        {
            return FindEntriesAsync(false, CancellationToken.None);
        }

        public Task<IClientWithRequest<T>> FindEntriesAsync(CancellationToken cancellationToken)
        {
            return FindEntriesAsync(false, cancellationToken);
        }

        public Task<IClientWithRequest<T>> FindEntriesAsync(bool scalarResult)
        {
            return FindEntriesAsync(scalarResult, CancellationToken.None);
        }

        public Task<IClientWithRequest<T>> FindEntriesAsync(bool scalarResult, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IClientWithRequest<T>> FindEntriesAsync(ODataFeedAnnotations annotations)
        {
            return FindEntriesAsync(annotations, CancellationToken.None);
        }

        public Task<IClientWithRequest<T>> FindEntriesAsync(ODataFeedAnnotations annotations, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IClientWithRequest<T>> FindEntriesAsync(Uri annotatedUri, ODataFeedAnnotations annotations)
        {
            return FindEntriesAsync(annotatedUri, annotations, CancellationToken.None);
        }

        public Task<IClientWithRequest<T>> FindEntriesAsync(Uri annotatedUri, ODataFeedAnnotations annotations, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IClientWithRequest<T>> FindEntryAsync()
        {
            return FindEntryAsync(CancellationToken.None);
        }

        public async Task<IClientWithRequest<T>> FindEntryAsync(CancellationToken cancellationToken)
        {
            return new ClientWithRequest<T>(await _builder.GetRequestAsync(false, cancellationToken), _session);
        }

        public Task<IClientWithRequest<T>> InsertEntryAsync()
        {
            return InsertEntryAsync(true, CancellationToken.None);
        }

        public Task<IClientWithRequest<T>> InsertEntryAsync(bool resultRequired)
        {
            return InsertEntryAsync(resultRequired, CancellationToken.None);
        }

        public Task<IClientWithRequest<T>> InsertEntryAsync(CancellationToken cancellationToken)
        {
            return InsertEntryAsync(true, cancellationToken);
        }

        public async Task<IClientWithRequest<T>> InsertEntryAsync(bool resultRequired, CancellationToken cancellationToken)
        {
            return new ClientWithRequest<T>(await _builder.InsertRequestAsync(resultRequired, cancellationToken), _session);
        }

        public Task<IClientWithRequest<T>> UpdateEntryAsync()
        {
            return UpdateEntryAsync(true, CancellationToken.None);
        }

        public Task<IClientWithRequest<T>> UpdateEntryAsync(bool resultRequired)
        {
            return UpdateEntryAsync(resultRequired, CancellationToken.None);
        }

        public Task<IClientWithRequest<T>> UpdateEntryAsync(CancellationToken cancellationToken)
        {
            return UpdateEntryAsync(true, cancellationToken);
        }

        public async Task<IClientWithRequest<T>> UpdateEntryAsync(bool resultRequired, CancellationToken cancellationToken)
        {
            return new ClientWithRequest<T>(await _builder.UpdateRequestAsync(resultRequired, cancellationToken), _session);
        }

        public Task<IClientWithRequest<T>> DeleteEntryAsync()
        {
            return DeleteEntryAsync(CancellationToken.None);
        }

        public async Task<IClientWithRequest<T>> DeleteEntryAsync(CancellationToken cancellationToken)
        {
            return new ClientWithRequest<T>(await _builder.DeleteRequestAsync(cancellationToken), _session);
        }
    }
}
