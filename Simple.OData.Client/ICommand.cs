using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    public interface ICommand
    {
        IClientWithCommand Collection(string collectionName);
        IClientWithCommand Filter(string filter);
        IClientWithCommand Skip(int count);
        IClientWithCommand Top(int count);
        IClientWithCommand Expand(IEnumerable<string> associations);
        IClientWithCommand Expand(params string[] associations);
        IClientWithCommand Select(IEnumerable<string> columns);
        IClientWithCommand Select(params string[] columns);
        IClientWithCommand OrderBy(IEnumerable<string> columns, bool descending = false);
        IClientWithCommand OrderBy(params string[] columns);
        IClientWithCommand OrderByDescending(IEnumerable<string> columns);
        IClientWithCommand OrderByDescending(params string[] columns);
    }
}
