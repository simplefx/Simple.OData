using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    interface IRequestWriter
    {
        Task<string> CreateEntryAsync(string operation, string entityTypeNamespace, string entityTypeName, 
            IDictionary<string, object> properties,
            IEnumerable<KeyValuePair<string, object>> associationsByValue,
            IEnumerable<KeyValuePair<string, int>> associationsByContentId);

        Task<string> CreateLinkAsync(string linkPath);
    }
}