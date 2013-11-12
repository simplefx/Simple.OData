using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    partial class ODataClientWithCommand
    {
        public Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync()
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesAsync(_command.ToString()), _command.SelectedColumns);
        }

        public Task<IEnumerable<IDictionary<string, object>>> FindEntriesAsync(bool scalarResult)
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesAsync(_command.ToString(), scalarResult), _command.SelectedColumns);
        }

        public Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync()
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesWithCountAsync(_command.WithInlineCount().ToString()), _command.SelectedColumns);
        }

        public Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> FindEntriesWithCountAsync(bool scalarResult)
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesWithCountAsync(_command.WithInlineCount().ToString(), scalarResult), _command.SelectedColumns);
        }

        public Task<IDictionary<string, object>> FindEntryAsync()
        {
            return RectifyColumnSelectionAsync(_client.FindEntryAsync(_command.ToString()), _command.SelectedColumns);
        }

        public Task<object> FindScalarAsync()
        {
            return _client.FindScalarAsync(_command.ToString());
        }

        public Task<IDictionary<string, object>> InsertEntryAsync(bool resultRequired = true)
        {
            return _client.InsertEntryAsync(_command.CollectionName, _command.EntryData, resultRequired);
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

        public Task LinkEntryAsync(string linkName, IDictionary<string, object> linkedEntryKey)
        {
            return _client.LinkEntryAsync(_command.CollectionName, _command.KeyValues, linkName, linkedEntryKey);
        }

        public Task LinkEntryAsync(ODataExpression expression, IDictionary<string, object> linkedEntryKey)
        {
            throw new NotImplementedException();
        }

        public Task UnlinkEntryAsync(string linkName)
        {
            return _client.UnlinkEntryAsync(_command.CollectionName, _command.KeyValues, linkName);
        }

        public Task UnlinkEntryAsync(ODataExpression expression)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IDictionary<string, object>>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters)
        {
            return RectifyColumnSelectionAsync(_client.ExecuteFunctionAsync(_command.ToString(), parameters), _command.SelectedColumns);
        }

        public Task<T> ExecuteFunctionAsync<T>(string functionName, IDictionary<string, object> parameters)
        {
            return _client.ExecuteFunctionAsScalarAsync<T>(_command.ToString(), parameters);
        }

        public Task<T> ExecuteFunctionAsScalarAsync<T>(string functionName, IDictionary<string, object> parameters)
        where T : class, new()
        {
            return _client.ExecuteFunctionAsScalarAsync<T>(_command.ToString(), parameters);
        }

        public Task<T[]> ExecuteFunctionAsArrayAsync<T>(string functionName, IDictionary<string, object> parameters)
        where T : class, new()
        {
            return _client.ExecuteFunctionAsArrayAsync<T>(_command.ToString(), parameters);
        }

        internal static Task<IEnumerable<IDictionary<string, object>>> RectifyColumnSelectionAsync(Task<IEnumerable<IDictionary<string, object>>> entries, IList<string> selectedColumns)
        {
            return entries.ContinueWith(x => RectifyColumnSelection(x.Result, selectedColumns));
        }

        internal static Task<IDictionary<string, object>> RectifyColumnSelectionAsync(Task<IDictionary<string, object>> entry, IList<string> selectedColumns)
        {
            return entry.ContinueWith(x => RectifyColumnSelection(x.Result, selectedColumns));
        }

        internal static Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> RectifyColumnSelectionAsync(Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> entries, IList<string> selectedColumns)
        {
            return entries.ContinueWith(x =>
            {
                var result = x.Result;
                return new Tuple<IEnumerable<IDictionary<string, object>>, int>(
                    RectifyColumnSelection(result.Item1, selectedColumns),
                    result.Item2);
            });
        }
    }
}
