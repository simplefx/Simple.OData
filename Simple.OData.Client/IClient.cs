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
        IDictionary<string, object> GetEntry(params object[] entryKey);
        IDictionary<string, object> GetEntry(IEnumerable<object> entryKey);
        IDictionary<string, object> GetEntry(IDictionary<string, object> entryKey);
        IDictionary<string, object> InsertEntry(IDictionary<string, object> entryData, bool resultRequired);
        int UpdateEntry(IDictionary<string, object> entryKey, IDictionary<string, object> entryData);
        int DeleteEntry(IDictionary<string, object> entryKey);
        void LinkEntry(IDictionary<string, object> entryKey, string linkName, IDictionary<string, object> linkedEntryKey);
        void UnlinkEntry(IDictionary<string, object> entryKey, string linkName);
        IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> ExecuteFunction(string functionName, IDictionary<string, object> parameters);
    }
}