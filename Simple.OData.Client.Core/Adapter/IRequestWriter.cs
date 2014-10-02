using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    public interface IRequestWriter
    {
        Task<ODataRequest> CreateInsertRequestAsync(string collection, IDictionary<string, object> entryData, bool resultRequired);
        Task<ODataRequest> CreateUpdateRequestAsync(string commandText, string collection, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired);
        Task<ODataRequest> CreateDeleteRequestAsync(string commandText, string collection);
        Task<ODataRequest> CreateLinkRequestAsync(string collection, string linkName, string entryKey, string linkKey);
        Task<ODataRequest> CreateUnlinkRequestAsync(string collection, string linkName, string entryKey);
    }
}
