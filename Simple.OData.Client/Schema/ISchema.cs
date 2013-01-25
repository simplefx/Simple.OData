using System.Collections.Generic;

namespace Simple.OData.Client
{
    public interface ISchema
    {
        string TypesNamespace { get; }
        string ContainersNamespace { get; }
        IEnumerable<Table> Tables { get; }
        bool HasTable(string tableName);
        Table FindTable(string tableName);
        Table FindBaseTable(string tablePath);
        Table FindConcreteTable(string tablePath);
        Column FindColumn(string tablePath, string columnName);
        Association FindAssociation(string tablePath, string associationName);
        IEnumerable<Function> Functions { get; }
        bool HasFunction(string functionName);
        Function FindFunction(string functionName);
        IEnumerable<EdmEntityType> EntityTypes { get; }
        IEnumerable<EdmComplexType> ComplexTypes { get; }
    }
}
