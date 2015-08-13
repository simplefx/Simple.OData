using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Simple.OData.Client
{
    /// <summary>
    /// Provides access to unbound OData operations in a fluent style.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IUnboundClient<T> : IFluentClient<T, IUnboundClient<T>> 
        where T : class
    {
        /// <summary>
        /// Casts the collection of base entities as the collection of derived ones.
        /// </summary>
        /// <param name="derivedCollectionName">Name of the derived collection.</param>
        /// <returns>Self.</returns>
        IUnboundClient<IDictionary<string, object>> As(string derivedCollectionName);
        /// <summary>
        /// Casts the collection of base entities as the collection of derived ones.
        /// </summary>
        /// <param name="derivedCollectionName">Name of the derived collection.</param>
        /// <returns>Self.</returns>
        IUnboundClient<U> As<U>(string derivedCollectionName = null) where U : class;
        /// <summary>
        /// Casts the collection of base entities as the collection of derived ones.
        /// </summary>
        /// <param name="expression">The expression for the derived collection.</param>
        /// <returns>Self.</returns>
        IUnboundClient<ODataEntry> As(ODataExpression expression);
    }
}