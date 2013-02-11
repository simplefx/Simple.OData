using System.Collections.Generic;

namespace Simple.OData.Client
{
    public interface ICommand
    {
        IClientWithCommand For(string collectionName);
        IClientWithCommand As(string derivedCollectionName);
        IClientWithCommand Key(params object[] entryKey);
        IClientWithCommand Key(IEnumerable<object> entryKey);
        IClientWithCommand Key(IDictionary<string, object> entryKey);
        IClientWithCommand Filter(string filter);
        IClientWithCommand Filter(FilterExpression expression);
        IClientWithCommand Skip(int count);
        IClientWithCommand Top(int count);
        IClientWithCommand Expand(IEnumerable<string> associations);
        IClientWithCommand Expand(params string[] associations);
        IClientWithCommand Select(IEnumerable<string> columns);
        IClientWithCommand Select(params string[] columns);
        IClientWithCommand OrderBy(IEnumerable<KeyValuePair<string, bool>> columns);
        IClientWithCommand OrderBy(params string[] columns);
        IClientWithCommand OrderByDescending(params string[] columns);
        IClientWithCommand Count();
        IClientWithCommand NavigateTo(string linkName);
        IClientWithCommand Set(object value);
        IClientWithCommand Set(IDictionary<string, object> value);
        IClientWithCommand Function(string functionName);
        IClientWithCommand Parameters(IDictionary<string, object> parameters);
        bool FilterIsKey { get; }
        IDictionary<string, object> FilterAsKey { get; }
    }
}
