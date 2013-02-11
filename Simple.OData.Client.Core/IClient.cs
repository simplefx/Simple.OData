using System.Collections.Generic;

namespace Simple.OData.Client
{
    public interface IClient
    {
        IEnumerable<IDictionary<string, object>> FindEntries();
        IEnumerable<IDictionary<string, object>> FindEntries(bool scalarResult);
        IEnumerable<IDictionary<string, object>> FindEntries(out int totalCount);
        IEnumerable<IDictionary<string, object>> FindEntries(bool scalarResult, out int totalCount);
        IDictionary<string, object> FindEntry();
        object FindScalar();
        IDictionary<string, object> InsertEntry(bool resultRequired = true);
        int UpdateEntry();
        int DeleteEntry();
        void LinkEntry(IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey);
        void UnlinkEntry(IDictionary<string, object> entryKey, string linkName);
        IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> ExecuteFunction(string functionName, IDictionary<string, object> parameters);
    }
}