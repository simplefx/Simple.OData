using System.Collections.Generic;

namespace Simple.OData.Client
{
    interface IRequestWriter
    {
        string CreateEntry(string entityTypeNamespace, string entityTypeName, 
            IDictionary<string, object> properties,
            IEnumerable<KeyValuePair<string, object>> associationsByValue,
            IEnumerable<KeyValuePair<string, int>> associationsByContentId);

        string CreateLink(string linkPath);
    }
}