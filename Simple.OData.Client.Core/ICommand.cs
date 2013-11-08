using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Simple.OData.Client
{
    public interface ICommand
    {
        IClientWithCommand As(string derivedCollectionName);
        IClientWithCommand As(ODataExpression expression);
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
        IClientWithCommand NavigateTo(ODataExpression expression);
        IClientWithCommand Set(object value);
        IClientWithCommand Set(IDictionary<string, object> value);
        IClientWithCommand Function(string functionName);
        IClientWithCommand Parameters(IDictionary<string, object> parameters);
        bool FilterIsKey { get; }
        IDictionary<string, object> FilterAsKey { get; }
    }

    public interface ICommand<T> : ICommand
        where T : class, new()
    {
        new IClientWithCommand<U> As<U>(string derivedCollectionName = null) where U : class, new();
        new IClientWithCommand<T> Key(params object[] entryKey);
        new IClientWithCommand<T> Key(IEnumerable<object> entryKey);
        new IClientWithCommand<T> Key(IDictionary<string, object> entryKey);
        new IClientWithCommand<T> Key(T entryKey);
        new IClientWithCommand<T> Filter(string filter);
        new IClientWithCommand<T> Filter(Expression<Func<T, bool>> expression);
        new IClientWithCommand<T> Expand(IEnumerable<string> associations);
        new IClientWithCommand<T> Expand(params string[] associations);
        new IClientWithCommand<T> Expand(Expression<Func<T, object>> expression);
        new IClientWithCommand<T> Select(IEnumerable<string> columns);
        new IClientWithCommand<T> Select(params string[] columns);
        new IClientWithCommand<T> Select(Expression<Func<T, object>> expression);
        new IClientWithCommand<T> OrderBy(IEnumerable<KeyValuePair<string, bool>> columns);
        new IClientWithCommand<T> OrderBy(params string[] columns);
        new IClientWithCommand<T> OrderBy(Expression<Func<T, object>> expression);
        new IClientWithCommand<T> OrderByDescending(params string[] columns);
        new IClientWithCommand<T> OrderByDescending(Expression<Func<T, object>> expression);
        new IClientWithCommand<U> NavigateTo<U>(string linkName = null) where U : class, new();
        new IClientWithCommand<T> Set(object value);
        new IClientWithCommand<T> Set(T entry);
    }
}
