using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Simple.OData.Client
{
    public interface IRequestWriter
    {
        Task<ODataRequest> CreateGetRequestAsync(string commandText, bool scalarResult);
        Task<ODataRequest> CreatePutRequestAsync(string commandText, Stream stream, string contentType);
        Task<ODataRequest> CreateInsertRequestAsync(string collection, IDictionary<string, object> entryData, bool resultRequired);
        Task<ODataRequest> CreateUpdateRequestAsync(string collection, string entryIdent, IDictionary<string, object> entryKey, IDictionary<string, object> entryData, bool resultRequired);
        Task<ODataRequest> CreateDeleteRequestAsync(string collection, string entryIdent);
        Task<ODataRequest> CreateLinkRequestAsync(string collection, string linkName, string entryIdent, string linkIdent);
        Task<ODataRequest> CreateUnlinkRequestAsync(string collection, string linkName, string entryIdent, string linkIdent);
        Task<ODataRequest> CreateFunctionRequestAsync(string collection, string functionName);
        Task<ODataRequest> CreateActionRequestAsync(string collection, string actionName, IDictionary<string, object> parameters, bool resultRequired);
    }
}
