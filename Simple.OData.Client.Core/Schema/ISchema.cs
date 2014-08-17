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
        /// Gets the schema tables.
        /// </summary>
        /// <value>
        /// The tables.
        /// </value>
        IEnumerable<EntitySet> EntitySets { get; }
        /// <summary>
        /// Determines whether the schema has the specified EntitySet.
        /// </summary>
        /// <param name="entitySetName">Name of the EntitySet.</param>
        /// <returns><c>true</c> if the schema contains the EntitySet; otherwise, <c>false</c>.</returns>
        bool HasTable(string entitySetName);
        /// <summary>
        /// Finds the specified EntitySet.
        /// </summary>
        /// <param name="entitySetName">Name of the EntitySet.</param>
        /// <returns>The EntitySet found.</returns>
        EntitySet FindEntitySet(string entitySetName);
        /// <summary>
        /// Finds the base EntitySet.
        /// </summary>
        /// <param name="entitySetPath">The EntitySet path.</param>
        /// <returns>The EntitySet found.</returns>
        EntitySet FindBaseEntitySet(string entitySetPath);
        /// <summary>
        /// Finds the concrete EntitySet.
        /// </summary>
        /// <param name="entitySetPath">The EntitySet path.</param>
        /// <returns>The EntitySet found.</returns>
        EntitySet FindConcreteEntitySet(string entitySetPath);
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
