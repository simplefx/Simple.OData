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

        public IUnboundClient<T> Skip(int count)
        {
            this.Command.Skip(count);
            return this;
        }

        public IUnboundClient<T> Top(int count)
        {
            this.Command.Top(count);
            return this;
        }

        public IUnboundClient<T> Expand(IEnumerable<string> columns)
        {
            this.Command.Expand(columns);
            return this;
        }

        public IUnboundClient<T> Expand(params string[] columns)
        {
            this.Command.Expand(columns);
            return this;
        }

        public IUnboundClient<T> Expand(params ODataExpression[] associations)
        {
            this.Command.Expand(associations);
            return this;
        }

        public IUnboundClient<T> Expand(Expression<Func<T, object>> expression)
        {
            this.Command.Expand(ExtractColumnNames(expression));
            return this;
        }

        public IUnboundClient<T> Select(IEnumerable<string> columns)
        {
            this.Command.Select(columns);
            return this;
        }

        public IUnboundClient<T> Select(params string[] columns)
        {
            this.Command.Select(columns);
            return this;
        }

        public IUnboundClient<T> Select(params ODataExpression[] columns)
        {
            this.Command.Select(columns);
            return this;
        }

        public IUnboundClient<T> Select(Expression<Func<T, object>> expression)
        {
            this.Command.Select(ExtractColumnNames(expression));
            return this;
        }

        public IUnboundClient<T> OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            this.Command.OrderBy(columns);
            return this;
        }

        public IUnboundClient<T> OrderBy(params string[] columns)
        {
            this.Command.OrderBy(columns);
            return this;
        }

        public IUnboundClient<T> OrderBy(params ODataExpression[] columns)
        {
            this.Command.OrderBy(columns);
            return this;
        }

        public IUnboundClient<T> OrderBy(Expression<Func<T, object>> expression)
        {
            this.Command.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, false)));
            return this;
        }

        public IUnboundClient<T> ThenBy(params ODataExpression[] columns)
        {
            this.Command.ThenBy(columns);
            return this;
        }

        public IUnboundClient<T> ThenBy(Expression<Func<T, object>> expression)
        {
            this.Command.ThenBy(ExtractColumnNames(expression).ToArray());
            return this;
        }

        public IUnboundClient<T> OrderByDescending(params string[] columns)
        {
            this.Command.OrderByDescending(columns);
            return this;
        }

        public IUnboundClient<T> OrderByDescending(params ODataExpression[] columns)
        {
            this.Command.OrderByDescending(columns);
            return this;
        }

        public IUnboundClient<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            this.Command.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, true)));
            return this;
        }

        public IUnboundClient<T> ThenByDescending(params ODataExpression[] columns)
        {
            this.Command.ThenByDescending(columns);
            return this;
        }

        public IUnboundClient<T> ThenByDescending(Expression<Func<T, object>> expression)
        {
            this.Command.ThenByDescending(ExtractColumnNames(expression).ToArray());
            return this;
        }

        public IUnboundClient<T> Count()
        {
            this.Command.Count();
            return this;
        }

        public IBoundClient<U> NavigateTo<U>(string linkName = null)
            where U : class
        {
            return this.Link<U>(this.Command, linkName);
        }

        public IBoundClient<U> NavigateTo<U>(Expression<Func<T, U>> expression)
            where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }

        public IBoundClient<U> NavigateTo<U>(Expression<Func<T, IEnumerable<U>>> expression) where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }

        public IBoundClient<U> NavigateTo<U>(Expression<Func<T, IList<U>>> expression) where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }

        public IBoundClient<U> NavigateTo<U>(Expression<Func<T, U[]>> expression) where U : class
        {
            return this.Link<U>(this.Command, ExtractColumnName(expression));
        }

        public IBoundClient<IDictionary<string, object>> NavigateTo(string linkName)
        {
            return this.Link<IDictionary<string, object>>(this.Command, linkName);
        }

        public IBoundClient<T> NavigateTo(ODataExpression expression)
        {
            return this.Link<T>(this.Command, expression);
        }
    }
}