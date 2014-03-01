using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    partial class FluentClient<T>
    {
        public Task<IEnumerable<T>> FindEntriesAsync()
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesAsync(_command), _command.SelectedColumns);
        }

        public Task<IEnumerable<T>> FindEntriesAsync(bool scalarResult)
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesAsync(_command, scalarResult), _command.SelectedColumns);
        }

        public async Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync()
        {
            var commandText = await _command.WithInlineCount().GetCommandTextAsync();
            var result = _client.FindEntriesWithCountAsync(commandText);
            return await RectifyColumnSelectionAsync(result, _command.SelectedColumns);
        }

        public async Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(bool scalarResult)
        {
            var commandText = await _command.WithInlineCount().GetCommandTextAsync();
            var result = _client.FindEntriesWithCountAsync(commandText, scalarResult);
            return await RectifyColumnSelectionAsync(result, _command.SelectedColumns);
        }

        public Task<T> FindEntryAsync()
        {
            return RectifyColumnSelectionAsync(_client.FindEntryAsync(_command), _command.SelectedColumns);
        }

        public Task<object> FindScalarAsync()
        {
            return _client.FindScalarAsync(_command);
        }

        public Task<T> InsertEntryAsync(bool resultRequired = true)
        {
            return _client.InsertEntryAsync(_command, _command.EntryData, resultRequired).ContinueWith(x =>
            {
                var result = x.Result;
                return result.ToObject<T>(_dynamicResults);
            });
        }

        public Task UpdateEntryAsync()
        {
            if (_command.HasFilter)
                return UpdateEntriesAsync();
            else
                return _client.UpdateEntryAsync(_command, _command.KeyValues, _command.EntryData);
        }

        public Task<int> UpdateEntriesAsync()
        {
            return _client.UpdateEntriesAsync(_command, _command.EntryData);
        }

        public Task DeleteEntryAsync()
        {
            if (_command.HasFilter)
                return DeleteEntriesAsync();
            else
                return _client.DeleteEntryAsync(_command, _command.KeyValues);
        }

        public Task<int> DeleteEntriesAsync()
        {
            return _client.DeleteEntriesAsync(_command);
        }

        public Task LinkEntryAsync<U>(U linkedEntryKey, string linkName = null)
        {
            return _client.LinkEntryAsync(_command, _command.KeyValues, linkName ?? typeof(U).Name, linkedEntryKey.ToDictionary());
        }

        public Task LinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey)
        {
            return _client.LinkEntryAsync(_command, _command.KeyValues, ExtractColumnName(expression), linkedEntryKey.ToDictionary());
        }

        public Task LinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey)
        {
            return _client.LinkEntryAsync(_command, _command.KeyValues, expression.AsString(), linkedEntryKey.ToDictionary());
        }

        public Task LinkEntryAsync(ODataExpression expression, ODataEntry linkedEntryKey)
        {
            return _client.LinkEntryAsync(_command, _command.KeyValues, expression.AsString(), linkedEntryKey.ToDictionary());
        }

        public Task UnlinkEntryAsync<U>(string linkName = null)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, linkName ?? typeof(U).Name);
        }

        public Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, ExtractColumnName(expression));
        }

        public Task UnlinkEntryAsync(ODataExpression expression)
        {
            return _client.UnlinkEntryAsync(_command, _command.KeyValues, expression.AsString());
        }

        public Task<IEnumerable<T>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters)
        {
            return RectifyColumnSelectionAsync(_client.ExecuteFunctionAsync(_command, parameters), _command.SelectedColumns);
        }

        public Task<T> ExecuteFunctionAsScalarAsync(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsScalarAsync<T>(_command, parameters);
        }

        public Task<T[]> ExecuteFunctionAsArrayAsync(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsArrayAsync<T>(_command, parameters);
        }

        public Task<string> GetCommandTextAsync()
        {
            return this.Command.GetCommandTextAsync();
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
