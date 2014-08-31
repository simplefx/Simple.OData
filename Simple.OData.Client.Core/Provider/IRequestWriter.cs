using System.Collections.Generic;

namespace Simple.OData.Client
{
    interface IRequestWriter
    {
        string CreateEntry(string entityTypeNamespace, string entityTypeName, 
            IDictionary<string, object> properties,
            IDictionary<string, object> associationsByValue,
            IDictionary<string, int> associationsByContentId);
    }
}