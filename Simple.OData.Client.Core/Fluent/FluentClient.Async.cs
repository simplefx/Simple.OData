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
            return RectifyColumnSelectionAsync(_client.FindEntriesAsync(_command.ToString()), _command.SelectedColumns);
        }

        public Task<IEnumerable<T>> FindEntriesAsync(bool scalarResult)
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesAsync(_command.ToString(), scalarResult), _command.SelectedColumns);
        }

        public Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync()
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesWithCountAsync(_command.WithInlineCount().ToString()), _command.SelectedColumns);
        }

        public Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(bool scalarResult)
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesWithCountAsync(_command.WithInlineCount().ToString(), scalarResult), _command.SelectedColumns);
        }

        public Task<T> FindEntryAsync()
        {
            return RectifyColumnSelectionAsync(_client.FindEntryAsync(_command.ToString()), _command.SelectedColumns);
        }

        public Task<object> FindScalarAsync()
        {
            return _client.FindScalarAsync(_command.ToString());
        }

        public Task<T> InsertEntryAsync(bool resultRequired = true)
        {
            return _client.InsertEntryAsync(_command.CollectionName, _command.EntryData, resultRequired).ContinueWith(x =>
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
                return _client.UpdateEntryAsync(_command.CollectionName, _command.KeyValues, _command.EntryData);
        }

        public Task<int> UpdateEntriesAsync()
        {
            return _client.UpdateEntriesAsync(_command.CollectionName, _command.ToString(), _command.EntryData);
        }

        public Task<int> DeleteEntryAsync()
        {
            if (_command.HasFilter)
                return DeleteEntriesAsync();
            else
                return _client.DeleteEntryAsync(_command.CollectionName, _command.KeyValues);
        }

        public Task<int> DeleteEntriesAsync()
        {
            return _client.DeleteEntriesAsync(_command.CollectionName, _command.ToString());
        }

        public Task LinkEntryAsync<U>(U linkedEntryKey, string linkName = null)
        {
            return _client.LinkEntryAsync(_command.CollectionName, _command.KeyValues, linkName ?? typeof(U).Name, linkedEntryKey.ToDictionary());
        }

        public Task LinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey)
        {
            return LinkEntryAsync(linkedEntryKey, ExtractColumnName(expression));
        }

        public Task LinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey)
        {
            return LinkEntryAsync(linkedEntryKey, expression.ToString());
        }

        public Task LinkEntryAsync(ODataExpression expression, ODataEntry linkedEntryKey)
        {
            return LinkEntryAsync(linkedEntryKey, expression.ToString());
        }

        public Task UnlinkEntryAsync<U>(string linkName = null)
        {
            return _client.UnlinkEntryAsync(_command.CollectionName, _command.KeyValues, linkName ?? typeof(U).Name);
        }

        public Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression)
        {
            return UnlinkEntryAsync(ExtractColumnName(expression));
        }

        public Task UnlinkEntryAsync(ODataExpression expression)
        {
            return UnlinkEntryAsync(expression.ToString());
        }

        public Task<IEnumerable<T>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters)
        {
            return RectifyColumnSelectionAsync(_client.ExecuteFunctionAsync(_command.ToString(), parameters), _command.SelectedColumns);
        }

        public Task<T> ExecuteFunctionAsScalarAsync(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsScalarAsync<T>(_command.ToString(), parameters);
        }

        public Task<T[]> ExecuteFunctionAsArrayAsync(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsArrayAsync<T>(_command.ToString(), parameters);
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
    }
}
