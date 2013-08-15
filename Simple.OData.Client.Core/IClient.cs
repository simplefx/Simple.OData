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
        int UpdateEntries();
        int DeleteEntry();
        int DeleteEntries();
        void LinkEntry(string linkName, IDictionary<string, object> linkedEntryKey);
        void UnlinkEntry(string linkName);
        IEnumerable<IDictionary<string, object>> ExecuteFunction(string functionName, IDictionary<string, object> parameters);
    }
}