using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Simple.OData.Client
{
    public interface ICommand
    {
        IClientWithCommand As(string derivedCollectionName);
        IClientWithCommand Key(params object[] entryKey);
        IClientWithCommand Key(IEnumerable<object> entryKey);
        IClientWithCommand Key(IDictionary<string, object> entryKey);
        IClientWithCommand Filter(string filter);
        IClientWithCommand Filter(ODataExpression expression);
        IClientWithCommand Skip(int count);
        IClientWithCommand Top(int count);
        IClientWithCommand Expand(IEnumerable<string> associations);
        IClientWithCommand Expand(params string[] associations);
        IClientWithCommand Expand(params ODataExpression[] associations);
        IClientWithCommand Select(IEnumerable<string> columns);
        IClientWithCommand Select(params string[] columns);
        IClientWithCommand Select(params ODataExpression[] columns);
        IClientWithCommand OrderBy(IEnumerable<KeyValuePair<string, bool>> columns);
        IClientWithCommand OrderBy(params string[] columns);
        IClientWithCommand OrderBy(params ODataExpression[] columns);
        IClientWithCommand OrderByDescending(params string[] columns);
        IClientWithCommand OrderByDescending(params ODataExpression[] columns);
        IClientWithCommand Count();
        IClientWithCommand NavigateTo(string linkName);
        IClientWithCommand Set(object value);
        IClientWithCommand Set(IDictionary<string, object> value);
        IClientWithCommand Function(string functionName);
        IClientWithCommand Parameters(IDictionary<string, object> parameters);
        bool FilterIsKey { get; }
        IDictionary<string, object> FilterAsKey { get; }
    }

    public interface ICommand<T> : ICommand
    {
        IClientWithCommand<U> As<U>(string derivedCollectionName = null);
        IClientWithCommand<T> Filter(Expression<Func<T, bool>> expression);
        IClientWithCommand<T> Expand(Expression<Func<T, object>> expression);
        IClientWithCommand<T> Select(Expression<Func<T, object>> expression);
        IClientWithCommand<T> OrderBy(Expression<Func<T, object>> expression);
        IClientWithCommand<T> OrderByDescending(Expression<Func<T, object>> expression);
        IClientWithCommand<U> NavigateTo<U>(string linkName = null);
    }
}
