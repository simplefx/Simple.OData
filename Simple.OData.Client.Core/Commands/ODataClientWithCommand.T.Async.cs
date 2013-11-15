using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    partial class ODataClientWithCommand<T>
    {
        public new Task<IEnumerable<T>> FindEntriesAsync()
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesAsync(_command.ToString()), _command.SelectedColumns);
        }

        public new Task<IEnumerable<T>> FindEntriesAsync(bool scalarResult)
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesAsync(_command.ToString(), scalarResult), _command.SelectedColumns);
        }

        public new Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync()
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesWithCountAsync(_command.WithInlineCount().ToString()), _command.SelectedColumns);
        }

        public new Task<Tuple<IEnumerable<T>, int>> FindEntriesWithCountAsync(bool scalarResult)
        {
            return RectifyColumnSelectionAsync(_client.FindEntriesWithCountAsync(_command.WithInlineCount().ToString(), scalarResult), _command.SelectedColumns);
        }

        public new Task<T> FindEntryAsync()
        {
            return RectifyColumnSelectionAsync(_client.FindEntryAsync(_command.ToString()), _command.SelectedColumns);
        }

        public new Task<T> InsertEntryAsync(bool resultRequired = true)
        {
            return _client.InsertEntryAsync(_command.CollectionName, _command.EntryData, resultRequired).ContinueWith(x =>
            {
                var result = x.Result;
                return result.ToObject<T>();
            });
        }

        public new Task LinkEntryAsync<U>(U linkedEntryKey, string linkName = null)
        {
            return _client.LinkEntryAsync(_command.CollectionName, _command.KeyValues, linkName ?? typeof(U).Name, linkedEntryKey.ToDictionary());
        }

        public new Task LinkEntryAsync<U>(Expression<Func<T, U>> expression, U linkedEntryKey)
        {
            return LinkEntryAsync(linkedEntryKey, ODataCommand.ExtractColumnName(expression));
        }

        public new Task LinkEntryAsync(ODataExpression expression, ODataEntry linkedEntryKey)
        {
            return LinkEntryAsync(linkedEntryKey, expression.ToString());
        }

        public new Task UnlinkEntryAsync<U>(string linkName = null)
        {
            return _client.UnlinkEntryAsync(_command.CollectionName, _command.KeyValues, linkName ?? typeof(U).Name);
        }

        public new Task UnlinkEntryAsync<U>(Expression<Func<T, U>> expression)
        {
            return UnlinkEntryAsync(ODataCommand.ExtractColumnName(expression));
        }

        public new Task UnlinkEntryAsync(ODataExpression expression)
        {
            return UnlinkEntryAsync(expression.ToString());
        }

        public new Task<IEnumerable<T>> ExecuteFunctionAsync(string functionName, IDictionary<string, object> parameters)
        {
            return RectifyColumnSelectionAsync(_client.ExecuteFunctionAsync(_command.ToString(), parameters), _command.SelectedColumns);
        }

        new internal static Task<IEnumerable<T>> RectifyColumnSelectionAsync(Task<IEnumerable<IDictionary<string, object>>> entries, IList<string> selectedColumns)
        {
            return entries.ContinueWith(x => RectifyColumnSelection(x.Result, selectedColumns).Select(y => y.ToObject<T>()));
        }

        new internal static Task<T> RectifyColumnSelectionAsync(Task<IDictionary<string, object>> entry, IList<string> selectedColumns)
        {
            return entry.ContinueWith(x => RectifyColumnSelection(x.Result, selectedColumns).ToObject<T>());
        }

        new internal static Task<Tuple<IEnumerable<T>, int>> RectifyColumnSelectionAsync(Task<Tuple<IEnumerable<IDictionary<string, object>>, int>> entries, IList<string> selectedColumns)
        {
            return entries.ContinueWith(x =>
            {
                var result = x.Result;
                return new Tuple<IEnumerable<T>, int>(
                    RectifyColumnSelection(result.Item1, selectedColumns).Select(y => y.ToObject<T>()),
                    result.Item2);
            });
        }
    }
}