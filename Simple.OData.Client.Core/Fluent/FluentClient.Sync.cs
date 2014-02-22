using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Simple.OData.Client
{
    partial class FluentClient<T>
    {
        public IEnumerable<T> FindEntries()
        {
            return Utils.ExecuteAndUnwrap(() => RectifyColumnSelectionAsync(_client.FindEntriesAsync(_command), _command.SelectedColumns));
        }

        public IEnumerable<T> FindEntries(bool scalarResult)
        {
            return Utils.ExecuteAndUnwrap(() => RectifyColumnSelectionAsync(_client.FindEntriesAsync(_command, scalarResult), _command.SelectedColumns));
        }

        public IEnumerable<T> FindEntries(out int totalCount)
        {
            var result = Utils.ExecuteAndUnwrap(FindEntriesWithCountAsync);
            totalCount = result.Item2;
            return result.Item1;
        }

        public IEnumerable<T> FindEntries(bool scalarResult, out int totalCount)
        {
            var result = Utils.ExecuteAndUnwrap(() => FindEntriesWithCountAsync(scalarResult));
            totalCount = result.Item2;
            return result.Item1;
        }

        public T FindEntry()
        {
            return Utils.ExecuteAndUnwrap(FindEntryAsync);
        }

        public object FindScalar()
        {
            return Utils.ExecuteAndUnwrap(FindScalarAsync);
        }

        public T InsertEntry(bool resultRequired = true)
        {
            return Utils.ExecuteAndUnwrap(() => InsertEntryAsync(resultRequired));
        }

        public int UpdateEntry()
        {
            return Utils.ExecuteAndUnwrap(UpdateEntryAsync);
        }

        public int UpdateEntries()
        {
            return Utils.ExecuteAndUnwrap(UpdateEntriesAsync);
        }

        public int DeleteEntry()
        {
            return Utils.ExecuteAndUnwrap(DeleteEntryAsync);
        }

        public int DeleteEntries()
        {
            return Utils.ExecuteAndUnwrap(DeleteEntriesAsync);
        }

        public void LinkEntry<U>(U linkedEntryKey, string linkName = null)
        {
            LinkEntryAsync(linkedEntryKey, linkName ?? typeof(U).Name).Wait();
        }

        public void LinkEntry<U>(Expression<Func<T, U>> expression, U linkedEntryKey)
        {
            LinkEntryAsync(expression, linkedEntryKey).Wait();
        }

        public void LinkEntry(ODataExpression expression, IDictionary<string, object> linkedEntryKey)
        {
            LinkEntryAsync(expression, linkedEntryKey).Wait();
        }

        public void LinkEntry(ODataExpression expression, ODataEntry linkedEntryKey)
        {
            LinkEntryAsync(expression, linkedEntryKey).Wait();
        }

        public void UnlinkEntry<U>(string linkName = null)
        {
            UnlinkEntryAsync(linkName ?? typeof(U).Name).Wait();
        }

        public void UnlinkEntry<U>(Expression<Func<T, U>> expression)
        {
            UnlinkEntryAsync(expression).Wait();
        }

        public void UnlinkEntry(ODataExpression expression)
        {
            UnlinkEntryAsync(expression).Wait();
        }

        public IEnumerable<T> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            return Utils.ExecuteAndUnwrap(() => ExecuteFunctionAsync(functionName, parameters));
        }

        public T ExecuteFunctionAsScalar(string functionName, IDictionary<string, object> parameters)
        {
            return Utils.ExecuteAndUnwrap(() => ExecuteFunctionAsScalarAsync(functionName, parameters));
        }

        public T[] ExecuteFunctionAsArray(string functionName, IDictionary<string, object> parameters)
        {
            return Utils.ExecuteAndUnwrap(() => ExecuteFunctionAsArrayAsync(functionName, parameters));
        }

        public string GetCommandText()
        {
            return Utils.ExecuteAndUnwrap(GetCommandTextAsync);
        }
    }
}
