using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;

namespace Simple.OData.Client
{
    public partial class ODataClient
    {
        public static ISchema GetSchema(string urlBase, ICredentials credentials = null)
        {
            return GetSchemaAsync(urlBase, credentials).Result;
        }

        public static string GetSchemaAsString(string urlBase, ICredentials credentials = null)
        {
            return Utils.ExecuteAndUnwrap(() => GetSchemaAsStringAsync(urlBase, credentials));
        }

        public ISchema GetSchema()
        {
            return GetSchemaAsync().Result;
        }

        public string GetSchemaAsString()
        {
            return Utils.ExecuteAndUnwrap(GetSchemaAsStringAsync);
        }

        public string GetCommandText(string collection, ODataExpression expression)
        {
            return Utils.ExecuteAndUnwrap(() => GetCommandTextAsync(collection, expression));
        }

        public string GetCommandText<T>(string collection, Expression<Func<T, bool>> expression)
        {
            return Utils.ExecuteAndUnwrap(() => GetCommandTextAsync(collection, expression));
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText)
        {
            return Utils.ExecuteAndUnwrap(() => FindEntriesAsync(commandText));
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult)
        {
            return Utils.ExecuteAndUnwrap(() => FindEntriesAsync(commandText, scalarResult));
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText, out int totalCount)
        {
            try
            {
                var result = FindEntriesWithCountAsync(commandText, false).Result;
                totalCount = result.Item2;
                return result.Item1;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult, out int totalCount)
        {
            try
            {
                var result = FindEntriesWithCountAsync(commandText, scalarResult).Result;
                totalCount = result.Item2;
                return result.Item1;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        public IDictionary<string, object> FindEntry(string commandText)
        {
            return Utils.ExecuteAndUnwrap(() => FindEntryAsync(commandText));
        }

        public object FindScalar(string commandText)
        {
            return Utils.ExecuteAndUnwrap(() => FindScalarAsync(commandText));
        }

        public IDictionary<string, object> GetEntry(string collection, params object[] entryKey)
        {
            return Utils.ExecuteAndUnwrap(() => GetEntryAsync(collection, entryKey));
        }

        public IDictionary<string, object> GetEntry(string collection, IDictionary<string, object> entryKey)
        {
            return Utils.ExecuteAndUnwrap(() => GetEntryAsync(collection, entryKey));
        }

        public IDictionary<string, object> InsertEntry(string collection, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            return Utils.ExecuteAndUnwrap(() => InsertEntryAsync(collection, entryData, resultRequired));
        }

        public IDictionary<string, object> UpdateEntry(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            return Utils.ExecuteAndUnwrap(() => UpdateEntryAsync(collection, entryKey, entryData, resultRequired));
        }

        public IEnumerable<IDictionary<string, object>> UpdateEntries(string collection, string commandText, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            return Utils.ExecuteAndUnwrap(() => UpdateEntriesAsync(collection, commandText, entryData, resultRequired));
        }

        public void DeleteEntry(string collection, IDictionary<string, object> entryKey)
        {
            Utils.ExecuteAndUnwrap(() => DeleteEntryAsync(collection, entryKey));
        }

        public int DeleteEntries(string collection, string commandText)
        {
            return Utils.ExecuteAndUnwrap(() => DeleteEntriesAsync(collection, commandText));
        }

        public void LinkEntry(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            Utils.ExecuteAndUnwrap(() => LinkEntryAsync(collection, entryKey, linkName, linkedEntryKey));
        }

        public void UnlinkEntry(string collection, IDictionary<string, object> entryKey, string linkName)
        {
            Utils.ExecuteAndUnwrap(() => UnlinkEntryAsync(collection, entryKey, linkName));
        }

        public IEnumerable<IDictionary<string, object>> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            return Utils.ExecuteAndUnwrap(() => ExecuteFunctionAsync(functionName, parameters));
        }

        public T ExecuteFunctionAsScalar<T>(string functionName, IDictionary<string, object> parameters)
        {
            return Utils.ExecuteAndUnwrap(() => ExecuteFunctionAsScalarAsync<T>(functionName, parameters));
        }

        public T[] ExecuteFunctionAsArray<T>(string functionName, IDictionary<string, object> parameters)
        {
            return Utils.ExecuteAndUnwrap(() => ExecuteFunctionAsArrayAsync<T>(functionName, parameters));
        }

        internal IEnumerable<IDictionary<string, object>> FindEntries(FluentCommand command)
        {
            return Utils.ExecuteAndUnwrap(() => FindEntriesAsync(command));
        }

        internal IEnumerable<IDictionary<string, object>> FindEntries(FluentCommand command, bool scalarResult)
        {
            return Utils.ExecuteAndUnwrap(() => FindEntriesAsync(command, scalarResult));
        }

        internal IEnumerable<IDictionary<string, object>> FindEntries(FluentCommand command, out int totalCount)
        {
            try
            {
                var result = FindEntriesWithCountAsync(command).Result;
                totalCount = result.Item2;
                return result.Item1;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        internal IEnumerable<IDictionary<string, object>> FindEntries(FluentCommand command, bool scalarResult, out int totalCount)
        {
            try
            {
                var result = FindEntriesWithCountAsync(command, scalarResult).Result;
                totalCount = result.Item2;
                return result.Item1;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        internal IDictionary<string, object> FindEntry(FluentCommand command)
        {
            return Utils.ExecuteAndUnwrap(() => FindEntryAsync(command));
        }

        internal object FindScalar(FluentCommand command)
        {
            return Utils.ExecuteAndUnwrap(() => FindScalarAsync(command));
        }

        internal IDictionary<string, object> InsertEntry(FluentCommand command, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            return Utils.ExecuteAndUnwrap(() => InsertEntryAsync(command, entryData, resultRequired));
        }

        public IDictionary<string, object> UpdateEntry(FluentCommand command, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            return Utils.ExecuteAndUnwrap(() => UpdateEntryAsync(command, entryKey, entryData, resultRequired));
        }

        internal IEnumerable<IDictionary<string, object>> UpdateEntries(FluentCommand command, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            return Utils.ExecuteAndUnwrap(() => UpdateEntriesAsync(command, entryData, resultRequired));
        }

        internal int DeleteEntries(FluentCommand command)
        {
            return Utils.ExecuteAndUnwrap(() => DeleteEntriesAsync(command));
        }

        public void DeleteEntry(FluentCommand command, IDictionary<string, object> entryKey)
        {
            Utils.ExecuteAndUnwrap(() => DeleteEntryAsync(command, entryKey));
        }

        public void LinkEntry(FluentCommand command, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            Utils.ExecuteAndUnwrap(() => LinkEntryAsync(command, entryKey, linkName, linkedEntryKey));
        }

        public void UnlinkEntry(FluentCommand command, IDictionary<string, object> entryKey, string linkName)
        {
            Utils.ExecuteAndUnwrap(() => UnlinkEntryAsync(command, entryKey, linkName));
        }

        internal IEnumerable<IDictionary<string, object>> ExecuteFunction(FluentCommand command, IDictionary<string, object> parameters)
        {
            return Utils.ExecuteAndUnwrap(() => ExecuteFunctionAsync(command, parameters));
        }

        internal T ExecuteFunctionAsScalar<T>(FluentCommand command, IDictionary<string, object> parameters)
        {
            return Utils.ExecuteAndUnwrap(() => ExecuteFunctionAsScalarAsync<T>(command, parameters));
        }

        internal T[] ExecuteFunctionAsArray<T>(FluentCommand command, IDictionary<string, object> parameters)
        {
            return Utils.ExecuteAndUnwrap(() => ExecuteFunctionAsArrayAsync<T>(command, parameters));
        }
    }
}
