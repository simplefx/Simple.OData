using System.Collections.Generic;

namespace Simple.OData.Client
{
    public interface IClientWithCommand : IClient, ICommand
    {
        string CommandText { get; }
    }
}
