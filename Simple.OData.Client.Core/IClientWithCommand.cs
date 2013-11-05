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
        new IClientWithCommand Set(object value);
        new IClientWithCommand Set(IDictionary<string, object> value);
    }

    public interface IClientWithCommand<T> : IClientWithCommand, ICommand<T>
    {
        // see http://stackoverflow.com/questions/10531575/passing-a-dynamic-parameter-throws-runtimebinderexception-when-calling-method-fr
        new IClientWithCommand<T> Filter(Expression<Func<T, bool>> expression);
    }
}
