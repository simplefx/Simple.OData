using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public class FunctionCollection : Collection<Function>
    {
        internal FunctionCollection()
        {
        }

        internal FunctionCollection(IEnumerable<Function> functions)
            : base(functions.ToList())
        {
        }

        public Function Find(string functionName)
        {
            var function = TryFind(functionName);

            if (function == null)
                throw new UnresolvableObjectException(functionName, string.Format("Function {0} not found", functionName));

            return function;
        }

        public bool Contains(string functionName)
        {
            return TryFind(functionName) != null;
        }

        private Function TryFind(string functionName)
        {
            functionName = functionName.Homogenize();
            return this.SingleOrDefault(f => f.HomogenizedName.Equals(functionName));
        }
    }
}