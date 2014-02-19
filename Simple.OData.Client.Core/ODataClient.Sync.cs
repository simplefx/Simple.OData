using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public partial class ODataClient
    {
        public static ISchema GetSchema(string urlBase, ICredentials credentials = null)
        {
            return Client.Schema.Get(urlBase, credentials);
        }

        public static string GetSchemaAsString(string urlBase, ICredentials credentials = null)
        {
            return ExecuteAndUnwrap(() => GetSchemaAsStringAsync(urlBase, credentials));
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText)
        {
            return ExecuteAndUnwrap(() => FindEntriesAsync(commandText));
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult)
        {
            return ExecuteAndUnwrap(() => FindEntriesAsync(commandText, scalarResult));
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
            return ExecuteAndUnwrap(() => FindEntryAsync(commandText));
        }

        public object FindScalar(string commandText)
        {
            return ExecuteAndUnwrap(() => FindScalarAsync(commandText));
        }

        public IDictionary<string, object> GetEntry(string collection, params object[] entryKey)
        {
            return ExecuteAndUnwrap(() => GetEntryAsync(collection, entryKey));
        }

        public IDictionary<string, object> GetEntry(string collection, IDictionary<string, object> entryKey)
        {
            return ExecuteAndUnwrap(() => GetEntryAsync(collection, entryKey));
        }

        public IDictionary<string, object> InsertEntry(string collection, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            return ExecuteAndUnwrap(() => InsertEntryAsync(collection, entryData, resultRequired));
        }

        public int UpdateEntries(string collection, string commandText, IDictionary<string, object> entryData)
        {
            return ExecuteAndUnwrap(() => UpdateEntriesAsync(collection, commandText, entryData));
        }

        public int UpdateEntry(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            return ExecuteAndUnwrap(() => UpdateEntryAsync(collection, entryKey, entryData));
        }

        public int DeleteEntries(string collection, string commandText)
        {
            return ExecuteAndUnwrap(() => DeleteEntriesAsync(collection, commandText));
        }

        public int DeleteEntry(string collection, IDictionary<string, object> entryKey)
        {
            return ExecuteAndUnwrap(() => DeleteEntryAsync(collection, entryKey));
        }

        public void LinkEntry(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            ExecuteAndUnwrap(() => LinkEntryAsync(collection, entryKey, linkName, linkedEntryKey));
        }

        public void UnlinkEntry(string collection, IDictionary<string, object> entryKey, string linkName)
        {
            ExecuteAndUnwrap(() => UnlinkEntryAsync(collection, entryKey, linkName));
        }

        public IEnumerable<IDictionary<string, object>> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteAndUnwrap(() => ExecuteFunctionAsync(functionName, parameters));
        }

        public T ExecuteFunctionAsScalar<T>(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteAndUnwrap(() => ExecuteFunctionAsScalarAsync<T>(functionName, parameters));
        }

        public T[] ExecuteFunctionAsArray<T>(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteAndUnwrap(() => ExecuteFunctionAsArrayAsync<T>(functionName, parameters));
        }

        private static T ExecuteAndUnwrap<T>(Func<Task<T>> func)
        {
            try
            {
                return func().Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        private static void ExecuteAndUnwrap(Func<Task> func)
        {
            try
            {
                func().Wait();
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }
    }
}
