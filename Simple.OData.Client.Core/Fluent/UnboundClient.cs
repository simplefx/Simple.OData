using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    /// <summary>
    /// Provides access to OData operations in a fluent style.
    /// </summary>
    /// <typeparam name="T">The entry type.</typeparam>
    public partial class UnboundClient<T> : FluentClientBase<T, IUnboundClient<T>>, IUnboundClient<T>
        where T : class
    {
        internal UnboundClient(ODataClient client, Session session, FluentCommand command = null, bool dynamicResults = false)
            : base(client, session, null, command, dynamicResults)
        {
        }

#pragma warning disable 1591

        public IUnboundClient<IDictionary<string, object>> As(string derivedCollectionName)
        {
            this.Command.As(derivedCollectionName);
            return new UnboundClient<IDictionary<string, object>>(_client, _session, this.Command, _dynamicResults);
        }

        public IUnboundClient<U> As<U>(string derivedCollectionName = null)
        where U : class
        {
            this.Command.As(derivedCollectionName ?? typeof(U).Name);
            return new UnboundClient<U>(_client, _session, this.Command, _dynamicResults);
        }

        public IUnboundClient<ODataEntry> As(ODataExpression expression)
        {
            this.Command.As(expression);
            return CreateClientForODataEntry();
        }

#pragma warning restore 1591

        private UnboundClient<ODataEntry> CreateClientForODataEntry()
        {
            return new UnboundClient<ODataEntry>(_client, _session, this.Command, true); ;
        }
    }
}