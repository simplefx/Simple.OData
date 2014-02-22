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
            return RectifyColumnSelectionAsync((_client as ODataClient).FindEntriesAsync(_command), _command.SelectedColumns);
        }

        public Task<IEnumerable<T>> FindEntriesAsync(bool scalarResult)
        {
            return RectifyColumnSelectionAsync((_client as ODataClient).FindEntriesAsync(_command, scalarResult), _command.SelectedColumns);
        }

        public async Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync1()
        {
            var commandText = await _command.WithInlineCount().GetCommandTextAsync();
            var result = _client.FindEntriesWithCountAsync(commandText);
            return await RectifyColumnSelectionAsync(result, _command.SelectedColumns);
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
            return RectifyColumnSelectionAsync((_client as ODataClient).FindEntryAsync(_command), _command.SelectedColumns);
        }

        public Task<object> FindScalarAsync()
        {
            return (_client as ODataClient).FindScalarAsync(_command);
        }

        public Task<T> InsertEntryAsync(bool resultRequired = true)
        {
            return (_client as ODataClient).InsertEntryAsync(_command, _command.EntryData, resultRequired).ContinueWith(x =>
            {
                var result = x.Result;
                return result.ToObject<T>(_dynamicResults);
            });
        }

        public Task<int> UpdateEntryAsync()
        {
            if (_command.HasFilter)
                return UpdateEntriesAsync();
            else
                return (_client as ODataClient).UpdateEntryAsync(_command, _command.KeyValues, _command.EntryData);
        }

        public Task<int> UpdateEntriesAsync()
        {
            return (_client as ODataClient).UpdateEntriesAsync(_command, _command.EntryData);
        }

        public Task<int> DeleteEntryAsync()
        {
            if (_command.HasFilter)
                return DeleteEntriesAsync();
            else
                return (_client as ODataClient).DeleteEntryAsync(_command, _command.KeyValues);
        }

        public Task<int> DeleteEntriesAsync()
        {
            return (_client as ODataClient).DeleteEntriesAsync(_command);
        }

        public Task LinkEntryAsync<U>(U linkedEntryKey, string linkName = null)
        {
            return (_client as ODataClient).LinkEntryAsync(_command, _command.KeyValues, linkName ?? typeof(U).Name, linkedEntryKey.ToDictionary());
        }

        public Task LinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey)
        {
            return LinkEntryAsync(linkedEntryKey, ExtractColumnName(expression));
        }

        public Task LinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey)
        {
            return LinkEntryAsync(linkedEntryKey, expression.AsString());
        }

        public Task LinkEntryAsync(ODataExpression expression, ODataEntry linkedEntryKey)
        {
            return LinkEntryAsync(linkedEntryKey, expression.AsString());
        }

        public Task UnlinkEntryAsync<U>(string linkName = null)
        {
            return (_client as ODataClient).UnlinkEntryAsync(_command, _command.KeyValues, linkName ?? typeof(U).Name);
        }

        public Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression)
        {
            return UnlinkEntryAsync(ExtractColumnName(expression));
        }

        public Task UnlinkEntryAsync(ODataExpression expression)
        {
            return UnlinkEntryAsync(expression.AsString());
        }

        public Task<IEnumerable<T>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters)
        {
            return RectifyColumnSelectionAsync((_client as ODataClient).ExecuteFunctionAsync(_command, parameters), _command.SelectedColumns);
        }

        public Task<T> ExecuteFunctionAsScalarAsync(string functionName, IDictionary<string, object> parameters)
        {
            return (_client as ODataClient).ExecuteFunctionAsScalarAsync<T>(_command, parameters);
        }

        public Task<T[]> ExecuteFunctionAsArrayAsync(string functionName, IDictionary<string, object> parameters)
        {
            return (_client as ODataClient).ExecuteFunctionAsArrayAsync<T>(_command, parameters);
        }

        public async Task<string> GetCommandTextAsync()
        {
            await ((_client as ODataClient).Schema as Schema).ResolveMetadataAsync();
            return await this.Command.GetCommandTextAsync();
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
