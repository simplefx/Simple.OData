using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Simple.OData.Client
{
    public interface ICommand<T>
        where T : class
    {
        IClientWithCommand<U> As<U>(string derivedCollectionName = null) where U : class;
        IClientWithCommand<ODataEntry> As(ODataExpression expression);
        IClientWithCommand<T> Key(params object[] entryKey);
        IClientWithCommand<T> Key(IEnumerable<object> entryKey);
        IClientWithCommand<T> Key(IDictionary<string, object> entryKey);
        IClientWithCommand<T> Key(T entryKey);
        IClientWithCommand<T> Filter(string filter);
        IClientWithCommand<ODataEntry> Filter(ODataExpression expression);
        IClientWithCommand<T> Filter(Expression<Func<T, bool>> expression);
        IClientWithCommand<T> Skip(int count);
        IClientWithCommand<T> Top(int count);
        IClientWithCommand<T> Expand(IEnumerable<string> associations);
        IClientWithCommand<T> Expand(params string[] associations);
        IClientWithCommand<ODataEntry> Expand(params ODataExpression[] associations);
        IClientWithCommand<T> Expand(Expression<Func<T, object>> expression);
        IClientWithCommand<T> Select(IEnumerable<string> columns);
        IClientWithCommand<T> Select(params string[] columns);
        IClientWithCommand<ODataEntry> Select(params ODataExpression[] columns);
        IClientWithCommand<T> Select(Expression<Func<T, object>> expression);
        IClientWithCommand<T> OrderBy(IEnumerable<KeyValuePair<string, bool>> columns);
        IClientWithCommand<T> OrderBy(params string[] columns);
        IClientWithCommand<ODataEntry> OrderBy(params ODataExpression[] columns);
        IClientWithCommand<T> OrderBy(Expression<Func<T, object>> expression);
        IClientWithCommand<T> ThenBy(Expression<Func<T, object>> expression);
        IClientWithCommand<T> OrderByDescending(params string[] columns);
        IClientWithCommand<ODataEntry> OrderByDescending(params ODataExpression[] columns);
        IClientWithCommand<T> OrderByDescending(Expression<Func<T, object>> expression);
        IClientWithCommand<T> ThenByDescending(Expression<Func<T, object>> expression);
        IClientWithCommand<T> Count();
        IClientWithCommand<U> NavigateTo<U>(string linkName = null) where U : class;
        IClientWithCommand<ODataEntry> NavigateTo(ODataExpression expression);
        IClientWithCommand<T> Set(object value);
        IClientWithCommand<T> Set(IDictionary<string, object> value);
        IClientWithCommand<ODataEntry> Set(params ODataExpression[] value);
        IClientWithCommand<T> Set(T entry);
        IClientWithCommand<T> Function(string functionName);
        IClientWithCommand<T> Parameters(IDictionary<string, object> parameters);
    }
}