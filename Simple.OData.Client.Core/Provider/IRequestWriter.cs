using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    interface IRequestWriter
    {
        Task<Stream> CreateEntryAsync(string method, string entityTypeNamespace, string entityTypeName, 
            IDictionary<string, object> properties, IEnumerable<ReferenceLink> links);

        Task<Stream> CreateLinkAsync(string linkPath);
    }
}