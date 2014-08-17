using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.OData.Client
{
    /// <summary>
    /// Access to OData service metadata
    /// </summary>
    public interface ISchema
    {
        /// <summary>
        /// Resolves the schema by requesting metada from the OData service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The schema.</returns>
        Task<ISchema> ResolveAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the schema metadata as string.
        /// </summary>
        /// <value>
        /// The metadata as string.
        /// </value>
        string MetadataAsString { get; }
        /// <summary>
        /// Gets the schema types namespace.
        /// </summary>
        /// <value>
        /// The types namespace.
        /// </value>
        [Obsolete("The value is unassigned. Use Namespace property for the specific type instead", false)]
        string TypesNamespace { get; }
        /// <summary>
        /// Gets the schema containers namespace.
        /// </summary>
        /// <value>
        /// The containers namespace.
        /// </value>
        [Obsolete("The value is unassigned.", false)]
        string ContainersNamespace { get; }
        /// <summary>
        /// Gets the schema tables.
        /// </summary>
        /// <value>
        /// The tables.
        /// </value>
        IEnumerable<Table> Tables { get; }
        /// <summary>
        /// Determines whether the schema has the specified table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns><c>true</c> if the schema contains the table; otherwise, <c>false</c>.</returns>
        bool HasTable(string tableName);
        /// <summary>
        /// Finds the specified table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>The table found.</returns>
        Table FindTable(string tableName);
        /// <summary>
        /// Finds the base table.
        /// </summary>
        /// <param name="tablePath">The table path.</param>
        /// <returns>The table found.</returns>
        Table FindBaseTable(string tablePath);
        /// <summary>
        /// Finds the concrete table.
        /// </summary>
        /// <param name="tablePath">The table path.</param>
        /// <returns>The table found.</returns>
        Table FindConcreteTable(string tablePath);
        /// <summary>
        /// Gets the schema entity types.
        /// </summary>
        /// <value>
        /// The entity types.
        /// </value>
        IEnumerable<EdmEntityType> EntityTypes { get; }
        /// <summary>
        /// Finds the type of the entity.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns>The entity type found.</returns>
        EdmEntityType FindEntityType(string typeName);
        /// <summary>
        /// Gets the schema complex types.
        /// </summary>
        /// <value>
        /// The complex types.
        /// </value>
        IEnumerable<EdmComplexType> ComplexTypes { get; }
        /// <summary>
        /// Finds the the complex type.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns>The complex type found.</returns>
        EdmComplexType FindComplexType(string typeName);
    }
}
