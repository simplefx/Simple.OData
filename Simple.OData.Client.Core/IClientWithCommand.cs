using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Simple.OData.Client
{
    public interface IClientWithCommand : IClient, ICommand
    {
        string CommandText { get; }

        // see http://stackoverflow.com/questions/10531575/passing-a-dynamic-parameter-throws-runtimebinderexception-when-calling-method-fr
        new IClientWithCommand Filter(string filter);
        new IClientWithCommand Filter(FilterExpression expression);
        new IClientWithCommand Expand(IEnumerable<string> associations);
        new IClientWithCommand Expand(params string[] associations);
        new IClientWithCommand Expand(params FilterExpression[] associations);
        new IClientWithCommand Select(IEnumerable<string> columns);
        new IClientWithCommand Select(params string[] columns);
        new IClientWithCommand Select(params FilterExpression[] columns);
        new IClientWithCommand OrderBy(IEnumerable<KeyValuePair<string, bool>> columns);
        new IClientWithCommand OrderBy(params string[] columns);
        new IClientWithCommand OrderBy(params FilterExpression[] columns);
        new IClientWithCommand OrderByDescending(params string[] columns);
        new IClientWithCommand OrderByDescending(params FilterExpression[] columns);
        new IClientWithCommand Set(object value);
        new IClientWithCommand Set(IDictionary<string, object> value);
    }

    public interface IClientWithCommand<T> : IClientWithCommand, ICommand<T>
    {
        // see http://stackoverflow.com/questions/10531575/passing-a-dynamic-parameter-throws-runtimebinderexception-when-calling-method-fr
        new IClientWithCommand<U> As<U>(string derivedCollectionName = null);
        new IClientWithCommand<T> Filter(Expression<Func<T, bool>> expression);
        new IClientWithCommand<T> Expand(Expression<Func<T, object>> expression);
        new IClientWithCommand<T> Select(Expression<Func<T, object>> expression);
        new IClientWithCommand<T> OrderBy(Expression<Func<T, object>> expression);
        new IClientWithCommand<T> OrderByDescending(Expression<Func<T, object>> expression);
        new IClientWithCommand<U> NavigateTo<U>(string linkName = null);
    }
}
