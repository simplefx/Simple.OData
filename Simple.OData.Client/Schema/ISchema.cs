using System.Collections.Generic;

namespace Simple.OData.Client
{
    public interface ISchema
    {
        string TypesNamespace { get; }
        string ContainersNamespace { get; }
        IEnumerable<Table> Tables { get; }
        Table FindTable(string tableName);
        bool HasTable(string tableName);
        IEnumerable<Function> Functions { get; }
        Function FindFunction(string functionName);
        bool HasFunction(string functionName);
        IEnumerable<EdmEntityType> EntityTypes { get; }
        IEnumerable<EdmComplexType> ComplexTypes { get; }
    }
}
