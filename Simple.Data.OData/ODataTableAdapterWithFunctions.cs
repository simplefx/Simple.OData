using System.Collections.Generic;

namespace Simple.Data.OData
{
    public partial class ODataTableAdapter : IAdapterWithFunctions
    {
        public bool IsValidFunction(string functionName)
        {
            return GetSchema().HasFunction(functionName);
        }

        public IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> Execute(string functionName, IDictionary<string, object> parameters)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IEnumerable<IEnumerable<KeyValuePair<string, object>>>> Execute(string functionName, IDictionary<string, object> parameters, IAdapterTransaction transaction)
        {
            throw new System.NotImplementedException();
        }
    }
}