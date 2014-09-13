using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    interface IRequestWriter
    {
        Task<Stream> WriteEntryContentAsync(string method, string collection, IDictionary<string, object> entryData, string commandText);
        Task<Stream> WriteLinkContentAsync(string linkPath);
    }
}