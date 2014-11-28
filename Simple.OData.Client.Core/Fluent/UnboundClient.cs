using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    /// <summary>
    /// Provides access to OData operations in a fluent style.
    /// </summary>
    /// <typeparam name="T">The entry type.</typeparam>
    public partial class UnboundClient<T> : FluentClientBase<T>, IUnboundClient<T>
        where T : class
    {
        internal UnboundClient(ODataClient client, Session session, FluentCommand command = null, bool dynamicResults = false)
            : base(client, session, null, command, dynamicResults)
        {
        }

        public IUnboundClient<T> Function(string functionName)
        {
            this.Command.Function(functionName);
            return this;
        }

        public IUnboundClient<T> Action(string actionName)
        {
            this.Command.Action(actionName);
            return this;
        }

        public IUnboundClient<T> Set(object value)
        {
            this.Command.Set(value);
            return this;
        }

        public IUnboundClient<T> Set(IDictionary<string, object> value)
        {
            this.Command.Set(value);
            return this;
        }

        public IUnboundClient<T> Set(params ODataExpression[] value)
        {
            this.Command.Set(value);
            return this;
        }
    }
}