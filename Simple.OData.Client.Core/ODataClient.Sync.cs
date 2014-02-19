using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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
            return SchemaProvider.FromUrl(urlBase, credentials).SchemaAsString;
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText)
        {
            try
            {
                return FindEntriesAsync(commandText).Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        public IEnumerable<IDictionary<string, object>> FindEntries(string commandText, bool scalarResult)
        {
            try
            {
                return FindEntriesAsync(commandText, scalarResult).Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
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
            try
            {
                return FindEntryAsync(commandText).Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        public object FindScalar(string commandText)
        {
            try
            {
                return FindScalarAsync(commandText).Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        public IDictionary<string, object> GetEntry(string collection, params object[] entryKey)
        {
            try
            {
                return GetEntryAsync(collection, entryKey).Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        public IDictionary<string, object> GetEntry(string collection, IDictionary<string, object> entryKey)
        {
            try
            {
                return GetEntryAsync(collection, entryKey).Result;
            }
            catch (AggregateException exception)
            {
                throw exception.InnerException;
            }
        }

        public IDictionary<string, object> InsertEntry(string collection, IDictionary<string, object> entryData, bool resultRequired = true)
        {
            return InsertEntryAsync(collection, entryData, resultRequired).Result;
        }

        public int UpdateEntries(string collection, string commandText, IDictionary<string, object> entryData)
        {
            return UpdateEntriesAsync(collection, commandText, entryData).Result;
        }

        public int UpdateEntry(string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData)
        {
            return UpdateEntryAsync(collection, entryKey, entryData).Result;
        }

        public int DeleteEntries(string collection, string commandText)
        {
            return DeleteEntriesAsync(collection, commandText).Result;
        }

        public int DeleteEntry(string collection, IDictionary<string, object> entryKey)
        {
            return DeleteEntryAsync(collection, entryKey).Result;
        }

        public void LinkEntry(string collection, IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey)
        {
            LinkEntryAsync(collection, entryKey, linkName, linkedEntryKey).Wait();
        }

        public void UnlinkEntry(string collection, IDictionary<string, object> entryKey, string linkName)
        {
            UnlinkEntryAsync(collection, entryKey, linkName).Wait();
        }

        public IEnumerable<IDictionary<string, object>> ExecuteFunction(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteFunctionAsync(functionName, parameters).Result;
        }

        public T ExecuteFunctionAsScalar<T>(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteFunctionAsScalarAsync<T>(functionName, parameters).Result;
        }

        public T[] ExecuteFunctionAsArray<T>(string functionName, IDictionary<string, object> parameters)
        {
            return ExecuteFunctionAsArrayAsync<T>(functionName, parameters).Result;
        }
    }
}
