using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    partial class FluentClient<T>
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

        public Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync()
        {
            return FindEntriesWithCountAsync(CancellationToken.None);
        }

        public async Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(CancellationToken cancellationToken)
        {
            var commandText = await _command.WithCount().GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            var result = _client.FindEntriesWithCountAsync(commandText, cancellationToken);
            return await RectifyColumnSelectionAsync(result, _command.SelectedColumns);
        }

        public Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(bool scalarResult)
        {
            return FindEntriesWithCountAsync(scalarResult, CancellationToken.None);
        }

        public async Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(bool scalarResult, CancellationToken cancellationToken)
        {
            var commandText = await _command.WithCount().GetCommandTextAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            var result = _client.FindEntriesWithCountAsync(commandText, scalarResult, cancellationToken);
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

        public Task<object> FindScalarAsync()
        {
            return FindScalarAsync(CancellationToken.None);
        }

        public Task<object> FindScalarAsync(CancellationToken cancellationToken)
        {
            return _client.FindScalarAsync(_command, cancellationToken);
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
            return _client.InsertEntryAsync(_command, _command.EntryData, resultRequired, cancellationToken).ContinueWith(x =>
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
            return _client.UpdateEntriesAsync(_command, _command.EntryData, resultRequired, cancellationToken).ContinueWith(x =>
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
            return _client.LinkEntryAsync(_command, _command.KeyValues, ExtractColumnName(expression), linkedEntryKey.ToDictionary(), cancellationToken);
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

        public Task UnlinkEntryAsync<U>(string linkName)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, linkName ?? typeof(U).Name, null, CancellationToken.None);
        }

        public Task UnlinkEntryAsync<U>(string linkName, CancellationToken cancellationToken)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, linkName ?? typeof(U).Name, null, cancellationToken);
        }

        public Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, ExtractColumnName(expression), null, CancellationToken.None);
        }

        public Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression, CancellationToken cancellationToken)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, ExtractColumnName(expression), null, cancellationToken);
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
            return UnlinkEntryAsync(linkedEntryKey, null, CancellationToken.None);
        }

        public Task UnlinkEntryAsync<U>(U linkedEntryKey, CancellationToken cancellationToken)
        {
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
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, ExtractColumnName(expression), linkedEntryKey != null ? linkedEntryKey.ToDictionary() : null, cancellationToken);
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

        public Task<IEnumerable<T>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters)
        {
            return RectifyColumnSelectionAsync(_client.ExecuteFunctionAsync(_command, parameters, CancellationToken.None), _command.SelectedColumns);
        }

        public Task<IEnumerable<T>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            return RectifyColumnSelectionAsync(_client.ExecuteFunctionAsync(_command, parameters, cancellationToken), _command.SelectedColumns);
        }

        public Task<T> ExecuteFunctionAsScalarAsync(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsScalarAsync<T>(_command, parameters, CancellationToken.None);
        }

        public Task<T> ExecuteFunctionAsScalarAsync(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            return _client.ExecuteFunctionAsScalarAsync<T>(_command, parameters, cancellationToken);
        }

        public Task<T[]> ExecuteFunctionAsArrayAsync(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteFunctionAsArrayAsync(functionName, parameters, CancellationToken.None);
        }

        public Task<T[]> ExecuteFunctionAsArrayAsync(string functionName, IDictionary<string, object> parameters, CancellationToken cancellationToken)
        {
            return _client.ExecuteFunctionAsArrayAsync<T>(_command, parameters, cancellationToken);
        }

        public Task<string> GetCommandTextAsync()
        {
            return GetCommandTextAsync(CancellationToken.None);
        }

        public Task<string> GetCommandTextAsync(CancellationToken cancellationToken)
        {
            return this.Command.GetCommandTextAsync(cancellationToken);
        }

        internal Task<IEnumerable<T>> RectifyColumnSelectionAsync(Task<IEnumerable<IDictionary<string, object>>> entries, IList<string> selectedColumns)
        {
            return entries.ContinueWith(
                x => RectifyColumnSelection(x.Result, selectedColumns)).ContinueWith(
                y => y.Result.Select(z => z.ToObject<T>(_dynamicResults)));
        }

        internal Task<T> RectifyColumnSelectionAsync(Task<IDictionary<string, object>> entry, IList<string> selectedColumns)
        {
            return entry.ContinueWith(
                x => RectifyColumnSelection(x.Result, selectedColumns).ToObject<T>(_dynamicResults));
        }

        internal Task<Tuple<IEnumerable<T>, int>> RectifyColumnSelectionAsync(Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> entries, IList<string> selectedColumns)
        {
            return entries.ContinueWith(x =>
            {
                var result = x.Result;
                return new Tuple<IEnumerable<T>, int>(
                    RectifyColumnSelection(result.Item1, selectedColumns).Select(y => y.ToObject<T>(_dynamicResults)),
                    result.Item2);
            });
        }

        internal static IEnumerable<IDictionary<string, object>> RectifyColumnSelection(IEnumerable<IDictionary<string, object>> entries, IList<string> selectedColumns)
        {
            return entries.Select(x => RectifyColumnSelection(x, selectedColumns));
        }

        internal static IDictionary<string, object> RectifyColumnSelection(IDictionary<string, object> entry, IList<string> selectedColumns)
        {
            if (selectedColumns == null || !selectedColumns.Any())
            {
                return entry;
            }
            else
            {
                return entry.Where(x => selectedColumns.Any(y => x.Key.Homogenize() == y.Homogenize())).ToIDictionary();
            }
        }
    }
}
