using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    partial class BoundClient<T>
    {
        public Task<IEnumerable<T>> FindEntriesAsync()
        {
            return FindEntriesAsync(CancellationToken.None);
        }

        public Task<IEnumerable<T>> FindEntriesAsync(CancellationToken cancellationToken)
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesAsync(_command, cancellationToken), _command.SelectedColumns);
        }

        public Task<IEnumerable<T>> FindEntriesAsync(bool scalarResult)
        {
            return FindEntriesAsync(scalarResult, CancellationToken.None);
        }

        public Task<IEnumerable<T>> FindEntriesAsync(bool scalarResult, CancellationToken cancellationToken)
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesAsync(_command, scalarResult, cancellationToken), _command.SelectedColumns);
        }

        public Task<IEnumerable<T>> FindEntriesAsync(ODataFeedAnnotations annotations)
        {
            return FindEntriesAsync(annotations, CancellationToken.None);
        }

        public async Task<IEnumerable<T>> FindEntriesAsync(ODataFeedAnnotations annotations, CancellationToken cancellationToken)
        {
            var commandText = await _command.WithCount().GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            var result = _client.FindEntriesAsync(commandText, annotations, cancellationToken);
            return await RectifyColumnSelectionAsync(result, _command.SelectedColumns);
        }

        public Task<IEnumerable<T>> FindEntriesAsync(Uri annotatedUri, ODataFeedAnnotations annotations)
        {
            return FindEntriesAsync(annotatedUri, annotations, CancellationToken.None);
        }

        public async Task<IEnumerable<T>> FindEntriesAsync(Uri annotatedUri, ODataFeedAnnotations annotations, CancellationToken cancellationToken)
        {
            var commandText = annotatedUri.AbsoluteUri;
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            var result = _client.FindEntriesAsync(commandText, annotations, cancellationToken);
            return await RectifyColumnSelectionAsync(result, _command.SelectedColumns);
        }

        public Task<T> FindEntryAsync()
        {
            return FindEntryAsync(CancellationToken.None);
        }

        public Task<T> FindEntryAsync(CancellationToken cancellationToken)
        {
            return RectifyColumnSelectionAsync(_client.FindEntryAsync(_command, cancellationToken), _command.SelectedColumns);
        }

        public Task<U> FindScalarAsync<U>()
        {
            return FindScalarAsync<U>(CancellationToken.None);
        }

        public Task<U> FindScalarAsync<U>(CancellationToken cancellationToken)
        {
            return _client.FindScalarAsync(_command, cancellationToken)
                .ContinueWith(x => (U)Convert.ChangeType(x.Result, typeof(U), CultureInfo.InvariantCulture), cancellationToken);
        }

        public Task<T> InsertEntryAsync()
        {
            return InsertEntryAsync(true, CancellationToken.None);
        }

        public Task<T> InsertEntryAsync(CancellationToken cancellationToken)
        {
            return InsertEntryAsync(true, cancellationToken);
        }

        public Task<T> InsertEntryAsync(bool resultRequired)
        {
            return InsertEntryAsync(resultRequired, CancellationToken.None);
        }

        public Task<T> InsertEntryAsync(bool resultRequired, CancellationToken cancellationToken)
        {
            return _client.InsertEntryAsync(_command, _command.CommandData, resultRequired, cancellationToken).ContinueWith(x =>
            {
                var result = x.Result;
                return result.ToObject<T>(_dynamicResults);
            }, cancellationToken);
        }

        public Task<T> UpdateEntryAsync()
        {
            return UpdateEntryAsync(true, CancellationToken.None);
        }

        public Task<T> UpdateEntryAsync(CancellationToken cancellationToken)
        {
            return UpdateEntryAsync(true, cancellationToken);
        }

        public Task<T> UpdateEntryAsync(bool resultRequired)
        {
            return UpdateEntryAsync(resultRequired, CancellationToken.None);
        }

        public async Task<T> UpdateEntryAsync(bool resultRequired, CancellationToken cancellationToken)
        {
            if (_command.HasFilter)
            {
                return await UpdateEntriesAsync(resultRequired, cancellationToken).ContinueWith(x =>
                {
                    if (resultRequired)
                    {
                        var result = x.Result;
                        return result == null ? null : result.First();
                    }
                    else
                    {
                        return null;
                    }
                }, cancellationToken);
            }
            else
            {
                return await _client.UpdateEntryAsync(_command, resultRequired, cancellationToken).ContinueWith(x =>
                {
                    if (resultRequired)
                    {
                        var result = x.Result;
                        return result == null ? null : result.ToObject<T>(_dynamicResults);
                    }
                    else
                    {
                        return null;
                    }
                }, cancellationToken);
            }
        }

        public Task<IEnumerable<T>> UpdateEntriesAsync()
        {
            return UpdateEntriesAsync(true, CancellationToken.None);
        }

        public Task<IEnumerable<T>> UpdateEntriesAsync(CancellationToken cancellationToken)
        {
            return UpdateEntriesAsync(true, cancellationToken);
        }

        public Task<IEnumerable<T>> UpdateEntriesAsync(bool resultRequired)
        {
            return UpdateEntriesAsync(resultRequired, CancellationToken.None);
        }

        public Task<IEnumerable<T>> UpdateEntriesAsync(bool resultRequired, CancellationToken cancellationToken)
        {
            return _client.UpdateEntriesAsync(_command, _command.CommandData, resultRequired, cancellationToken).ContinueWith(x =>
            {
                var result = x.Result;
                return result.Select(y => y.ToObject<T>(_dynamicResults));
            }, cancellationToken);
        }

        public Task DeleteEntryAsync()
        {
            return DeleteEntryAsync(CancellationToken.None);
        }

        public Task DeleteEntryAsync(CancellationToken cancellationToken)
        {
            if (_command.HasFilter)
                return DeleteEntriesAsync(cancellationToken);
            else
                return _client.DeleteEntryAsync(_command, cancellationToken);
        }

        public Task<int> DeleteEntriesAsync()
        {
            return DeleteEntriesAsync(CancellationToken.None);
        }

        public Task<int> DeleteEntriesAsync(CancellationToken cancellationToken)
        {
            return _client.DeleteEntriesAsync(_command, cancellationToken);
        }

        public Task LinkEntryAsync<U>(U linkedEntryKey)
        {
            return LinkEntryAsync(linkedEntryKey, null, CancellationToken.None);
        }

        public Task LinkEntryAsync<U>(U linkedEntryKey, CancellationToken cancellationToken)
        {
            return LinkEntryAsync(linkedEntryKey, null, cancellationToken);
        }

        public Task LinkEntryAsync<U>(U linkedEntryKey, string linkName)
        {
            return LinkEntryAsync(linkedEntryKey, linkName, CancellationToken.None);
        }

        public Task LinkEntryAsync<U>(U linkedEntryKey, string linkName, CancellationToken cancellationToken)
        {
            return _client.LinkEntryAsync(_command, _command.KeyValues, linkName ?? typeof(U).Name, linkedEntryKey.ToDictionary(), cancellationToken);
        }

        public Task LinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey)
        {
            return LinkEntryAsync(expression, linkedEntryKey, CancellationToken.None);
        }

        public Task LinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey, CancellationToken cancellationToken)
        {
            return _client.LinkEntryAsync(_command, _command.KeyValues, ColumnExpression.ExtractColumnName(expression), linkedEntryKey.ToDictionary(), cancellationToken);
        }

        public Task LinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey)
        {
            return _client.LinkEntryAsync(_command, _command.KeyValues, expression.AsString(_session), linkedEntryKey.ToDictionary(), CancellationToken.None);
        }

        public Task LinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken)
        {
            return _client.LinkEntryAsync(_command, _command.KeyValues, expression.AsString(_session), linkedEntryKey.ToDictionary(), cancellationToken);
        }

        public Task LinkEntryAsync(ODataExpression expression, ODataEntry linkedEntryKey)
        {
            return _client.LinkEntryAsync(_command, _command.KeyValues, expression.AsString(_session), linkedEntryKey.ToDictionary(), CancellationToken.None);
        }

        public Task LinkEntryAsync(ODataExpression expression, ODataEntry linkedEntryKey, CancellationToken cancellationToken)
        {
            return _client.LinkEntryAsync(_command, _command.KeyValues, expression.AsString(_session), linkedEntryKey.ToDictionary(), cancellationToken);
        }

        public Task UnlinkEntryAsync<U>()
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, typeof(U).Name, null, CancellationToken.None);
        }

        public Task UnlinkEntryAsync<U>(CancellationToken cancellationToken)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, typeof(U).Name, null, cancellationToken);
        }

        public Task UnlinkEntryAsync(string linkName)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, linkName, null, CancellationToken.None);
        }

        public Task UnlinkEntryAsync(string linkName, CancellationToken cancellationToken)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, linkName, null, cancellationToken);
        }

        public Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, ColumnExpression.ExtractColumnName(expression), null, CancellationToken.None);
        }

        public Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression, CancellationToken cancellationToken)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, ColumnExpression.ExtractColumnName(expression), null, cancellationToken);
        }

        public Task UnlinkEntryAsync(ODataExpression expression)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, expression.AsString(_session), null, CancellationToken.None);
        }

        public Task UnlinkEntryAsync(ODataExpression expression, CancellationToken cancellationToken)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, expression.AsString(_session), null, cancellationToken);
        }

        public Task UnlinkEntryAsync<U>(U linkedEntryKey)
        {
            return UnlinkEntryAsync(linkedEntryKey, CancellationToken.None);
        }

        public Task UnlinkEntryAsync<U>(U linkedEntryKey, CancellationToken cancellationToken)
        {
            if (linkedEntryKey.GetType() == typeof(string))
                return UnlinkEntryAsync(linkedEntryKey.ToString(), cancellationToken);
            else if (linkedEntryKey is ODataExpression && (linkedEntryKey as ODataExpression).Reference != null)
                return UnlinkEntryAsync((linkedEntryKey as ODataExpression).Reference, cancellationToken);
            else
                return UnlinkEntryAsync(linkedEntryKey, null, cancellationToken);
        }

        public Task UnlinkEntryAsync<U>(U linkedEntryKey, string linkName)
        {
            return UnlinkEntryAsync(linkedEntryKey, linkName, CancellationToken.None);
        }

        public Task UnlinkEntryAsync<U>(U linkedEntryKey, string linkName, CancellationToken cancellationToken)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, linkName ?? typeof(U).Name, linkedEntryKey != null ? linkedEntryKey.ToDictionary() : null, cancellationToken);
        }

        public Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey)
        {
            return UnlinkEntryAsync(expression, linkedEntryKey, CancellationToken.None);
        }

        public Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey, CancellationToken cancellationToken)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, ColumnExpression.ExtractColumnName(expression), linkedEntryKey != null ? linkedEntryKey.ToDictionary() : null, cancellationToken);
        }

        public Task UnlinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, expression.AsString(_session), linkedEntryKey != null ? linkedEntryKey.ToDictionary() : null, CancellationToken.None);
        }

        public Task UnlinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey, CancellationToken cancellationToken)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, expression.AsString(_session), linkedEntryKey != null ? linkedEntryKey.ToDictionary() : null, cancellationToken);
        }

        public Task UnlinkEntryAsync(ODataExpression expression, ODataEntry linkedEntryKey)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, expression.AsString(_session), linkedEntryKey != null ? linkedEntryKey.ToDictionary() : null, CancellationToken.None);
        }

        public Task UnlinkEntryAsync(ODataExpression expression, ODataEntry linkedEntryKey, CancellationToken cancellationToken)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, expression.AsString(_session), linkedEntryKey != null ? linkedEntryKey.ToDictionary() : null, cancellationToken);
        }
    }
}
