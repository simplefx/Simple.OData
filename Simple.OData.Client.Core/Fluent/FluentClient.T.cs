using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    public partial class FluentClient<T> : FluentClient, IFluentClient<T> where T : class
    {
        public FluentClient(ODataClient client, ODataCommand parent = null)
            : base(client, parent)
        {
        }

        internal FluentClient(FluentClient ancestor, ODataCommand command)
            : base(ancestor, new ODataCommand(command))
        {
        }

        protected override ODataCommand CreateCommand()
        {
            return new ODataCommand(this.Schema, _parent);
        }

        public FluentClient<U> Link<U>(ODataCommand command, string linkName = null)
        where U : class
        {
            var linkedClient = new FluentClient<U>(_client, command);
            linkedClient.Command.Link(linkName ?? typeof(U).Name);
            return linkedClient;
        }

        public new IFluentClient<T> For(string collectionName = null)
        {
            this.Command.For(collectionName ?? typeof(T).Name);
            return this;
        }

        public new IFluentClient<ODataEntry> For(ODataExpression expression)
        {
            this.Command.For(expression.Reference);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<U> As<U>(string derivedCollectionName = null)
        where U : class
        {
            this.Command.As(derivedCollectionName ?? typeof(U).Name);
            return new FluentClient<U>(this, this.Command);
        }

        public new IFluentClient<ODataEntry> As(ODataExpression expression)
        {
            this.Command.As(expression.ToString());
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public new IFluentClient<T> Key(params object[] entryKey)
        {
            this.Command.Key(entryKey);
            return this;
        }

        public new IFluentClient<T> Key(IEnumerable<object> entryKey)
        {
            this.Command.Key(entryKey);
            return this;
        }

        public new IFluentClient<T> Key(IDictionary<string, object> entryKey)
        {
            this.Command.Key(entryKey);
            return this;
        }

        public IFluentClient<T> Key(T entryKey)
        {
            this.Command.Key(entryKey.ToDictionary());
            return this;
        }

        public new IFluentClient<T> Filter(string filter)
        {
            this.Command.Filter(filter);
            return this;
        }

        public new IFluentClient<ODataEntry> Filter(ODataExpression expression)
        {
            this.Command.Filter(expression);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> Filter(Expression<Func<T, bool>> expression)
        {
            this.Command.Filter(ODataExpression.FromLinqExpression(expression.Body));
            return this;
        }

        public new IFluentClient<T> Skip(int count)
        {
            this.Command.Skip(count);
            return this;
        }

        public new IFluentClient<T> Top(int count)
        {
            this.Command.Top(count);
            return this;
        }

        public new IFluentClient<T> Expand(IEnumerable<string> columns)
        {
            this.Command.Expand(columns);
            return this;
        }

        public new IFluentClient<T> Expand(params string[] columns)
        {
            this.Command.Expand(columns);
            return this;
        }

        public new IFluentClient<ODataEntry> Expand(params ODataExpression[] associations)
        {
            this.Command.Expand(associations);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> Expand(Expression<Func<T, object>> expression)
        {
            this.Command.Expand(ExtractColumnNames(expression));
            return this;
        }

        public new IFluentClient<T> Select(IEnumerable<string> columns)
        {
            this.Command.Select(columns);
            return this;
        }

        public new IFluentClient<T> Select(params string[] columns)
        {
            this.Command.Select(columns);
            return this;
        }

        public new IFluentClient<ODataEntry> Select(params ODataExpression[] columns)
        {
            this.Command.Select(columns);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> Select(Expression<Func<T, object>> expression)
        {
            this.Command.Select(ExtractColumnNames(expression));
            return this;
        }

        public new IFluentClient<T> OrderBy(IEnumerable<KeyValuePair<string, bool>> columns)
        {
            this.Command.OrderBy(columns);
            return this;
        }

        public new IFluentClient<T> OrderBy(params string[] columns)
        {
            this.Command.OrderBy(columns);
            return this;
        }

        public new IFluentClient<ODataEntry> OrderBy(params ODataExpression[] columns)
        {
            this.Command.OrderBy(columns);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> OrderBy(Expression<Func<T, object>> expression)
        {
            this.Command.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, false)));
            return this;
        }

        public new IFluentClient<ODataEntry> ThenBy(params ODataExpression[] columns)
        {
            this.Command.ThenBy(columns);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> ThenBy(Expression<Func<T, object>> expression)
        {
            this.Command.ThenBy(ExtractColumnNames(expression).ToArray());
            return this;
        }

        public new IFluentClient<T> OrderByDescending(params string[] columns)
        {
            this.Command.OrderByDescending(columns);
            return this;
        }

        public new IFluentClient<ODataEntry> OrderByDescending(params ODataExpression[] columns)
        {
            this.Command.OrderByDescending(columns);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            this.Command.OrderBy(ExtractColumnNames(expression).Select(x => new KeyValuePair<string, bool>(x, true)));
            return this;
        }

        public new IFluentClient<ODataEntry> ThenByDescending(params ODataExpression[] columns)
        {
            this.Command.ThenByDescending(columns);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> ThenByDescending(Expression<Func<T, object>> expression)
        {
            this.Command.ThenByDescending(ExtractColumnNames(expression).ToArray());
            return this;
        }

        public new IFluentClient<T> Count()
        {
            this.Command.Count();
            return this;
        }

        public IFluentClient<U> NavigateTo<U>(string linkName = null)
        where U : class
        {
            return this.Link<U>(this.Command, linkName);
        }

        public new IFluentClient<ODataEntry> NavigateTo(ODataExpression expression)
        {
            return this.Link<ODataEntry>(this.Command, expression.ToString());
        }

        public new IFluentClient<T> Set(object value)
        {
            this.Command.Set(value);
            return this;
        }

        public new IFluentClient<T> Set(IDictionary<string, object> value)
        {
            this.Command.Set(value);
            return this;
        }

        public new IFluentClient<ODataEntry> Set(params ODataExpression[] value)
        {
            this.Command.Set(value);
            return new FluentClient<ODataEntry>(this, this.Command);
        }

        public IFluentClient<T> Set(T entry)
        {
            this.Command.Set(entry);
            return this;
        }

        public new IFluentClient<T> Function(string functionName)
        {
            this.Command.Function(functionName);
            return this;
        }

        public new IFluentClient<T> Parameters(IDictionary<string, object> parameters)
        {
            this.Command.Parameters(parameters);
            return this;
        }

        protected internal static IEnumerable<string> ExtractColumnNames(Expression<Func<T, object>> expression)
        {
            var lambdaExpression = Utils.CastExpressionWithTypeCheck<LambdaExpression>(expression);
            switch (lambdaExpression.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                case ExpressionType.Convert:
                    return new[] { ExtractColumnName(lambdaExpression.Body) };

                case ExpressionType.New:
                    var newExpression = lambdaExpression.Body as NewExpression;
                    return newExpression.Arguments.Select(ExtractColumnName);

                default:
                    throw Utils.NotSupportedExpression(lambdaExpression.Body);
            }
        }

        protected internal static string ExtractColumnName(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return (expression as MemberExpression).Member.Name;

                case ExpressionType.Convert:
                    return ExtractColumnName((expression as UnaryExpression).Operand);

                default:
                    throw Utils.NotSupportedExpression(expression);
            }
        }
    }
}