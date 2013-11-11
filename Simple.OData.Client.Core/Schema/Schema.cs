using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class Schema : ISchema
    {
        private static readonly SimpleDictionary<string, ISchema> Instances = new SimpleDictionary<string, ISchema>();

        private readonly SchemaProvider _schemaProvider;
        private readonly string _typesNamespace;
        private readonly string _containersNamespace;
        private readonly Lazy<TableCollection> _lazyTables;
        private readonly Lazy<FunctionCollection> _lazyFunctions;
        private readonly Lazy<List<EdmEntityType>> _lazyEntityTypes;
        private readonly Lazy<List<EdmComplexType>> _lazyComplexTypes;

        private Schema(SchemaProvider schemaProvider)
        {
            _schemaProvider = schemaProvider;
            _typesNamespace = _schemaProvider.GetTypesNamespace();
            _containersNamespace = _schemaProvider.GetContainersNamespace();
            _lazyTables = new Lazy<TableCollection>(CreateTableCollection);
            _lazyFunctions = new Lazy<FunctionCollection>(CreateFunctionCollection);
            _lazyEntityTypes = new Lazy<List<EdmEntityType>>(CreateEntityTypeCollection);
            _lazyComplexTypes = new Lazy<List<EdmComplexType>>(CreateComplexTypeCollection);
        }

        internal SchemaProvider SchemaProvider
        {
            get { return _schemaProvider; }
        }

        public string TypesNamespace
        {
            get { return _typesNamespace; }
        }

        public string ContainersNamespace
        {
            get { return _containersNamespace; }
        }

        public IEnumerable<Table> Tables
        {
            get { return _lazyTables.Value.AsEnumerable(); }
        }

        public bool HasTable(string tableName)
        {
            return _lazyTables.Value.Contains(tableName);
        }

        public Table FindTable(string tableName)
        {
            return _lazyTables.Value.Find(tableName);
        }

        public Table FindBaseTable(string tablePath)
        {
            return this.FindTable(tablePath.Split('/').First());
        }

        public Table FindConcreteTable(string tablePath)
        {
            var items = tablePath.Split('/');
            if (items.Count() > 1)
            {
                var baseTable = this.FindTable(items[0]);
                var table = string.IsNullOrEmpty(items[1])
                    ? baseTable
                    : baseTable.FindDerivedTable(items[1]);
                return table;
            }
            else
            {
                return this.FindTable(tablePath);
            }
        }

        public Column FindColumn(string tablePath, string columnName)
        {
            var baseTable = this.FindBaseTable(tablePath);
            var concreteTable = this.FindConcreteTable(tablePath);
            if (baseTable == concreteTable)
            {
                return concreteTable.FindColumn(columnName);
            }
            else
            {
                if (concreteTable.HasAssociation(columnName))
                    return concreteTable.FindColumn(columnName);
                else
                    return baseTable.FindColumn(columnName);
            }
        }

        public Association FindAssociation(string tablePath, string associationName)
        {
            var baseTable = this.FindBaseTable(tablePath);
            var concreteTable = this.FindConcreteTable(tablePath);
            if (baseTable == concreteTable)
            {
                return concreteTable.FindAssociation(associationName);
            }
            else
            {
                if (concreteTable.HasAssociation(associationName))
                    return concreteTable.FindAssociation(associationName);
                else
                    return baseTable.FindAssociation(associationName);
            }
        }

        public IEnumerable<Function> Functions
        {
            get { return _lazyFunctions.Value.AsEnumerable(); }
        }

        public bool HasFunction(string functionName)
        {
            return _lazyFunctions.Value.Contains(functionName);
        }

        public Function FindFunction(string functionName)
        {
            return _lazyFunctions.Value.Find(functionName);
        }

        public IEnumerable<EdmEntityType> EntityTypes
        {
            get { return _lazyEntityTypes.Value.AsEnumerable(); }
        }

        public EdmEntityType FindEntityType(string typeName)
        {
            Func<string, EdmEntityType> TryFind = x =>
            {
                x = x.Homogenize();
                return _lazyEntityTypes.Value.SingleOrDefault(t => t.Name.Homogenize().Equals(x));
            };

            var entityType = TryFind(typeName)
                   ?? TryFind(typeName.Singularize())
                   ?? TryFind(typeName.Pluralize());

            if (entityType == null)
                throw new UnresolvableObjectException(typeName, string.Format("Entity type {0} not found", typeName));

            return entityType;
        }

        public IEnumerable<EdmComplexType> ComplexTypes
        {
            get { return _lazyComplexTypes.Value.AsEnumerable(); }
        }

        public EdmComplexType FindComplexType(string typeName)
        {
            Func<string, EdmComplexType> TryFind = x =>
            {
                x = x.Homogenize();
                return _lazyComplexTypes.Value.SingleOrDefault(t => t.Name.Homogenize().Equals(x));
            };

            var complexType = TryFind(typeName)
                   ?? TryFind(typeName.Singularize())
                   ?? TryFind(typeName.Pluralize());

            if (complexType == null)
                throw new UnresolvableObjectException(typeName, string.Format("Complex type {0} not found", typeName));

            return complexType;
        }

        private TableCollection CreateTableCollection()
        {
            return new TableCollection(_schemaProvider.GetTables()
                .Select(table => new Table(table.ActualName, table.EntityType, null, this)));
        }

        private FunctionCollection CreateFunctionCollection()
        {
            return new FunctionCollection(_schemaProvider.GetFunctions());
        }

        private List<EdmEntityType> CreateEntityTypeCollection()
        {
            return new List<EdmEntityType>(_schemaProvider.GetEntityTypes());
        }

        private List<EdmComplexType> CreateComplexTypeCollection()
        {
            return new List<EdmComplexType>(_schemaProvider.GetComplexTypes());
        }

        internal static ISchema Get(string urlBase, ICredentials credentials = null)
        {
            return Instances.GetOrAdd(urlBase, sp => new Schema(Client.SchemaProvider.FromUrl(urlBase, credentials)));
        }

        internal static ISchema Get(SchemaProvider schemaProvider)
        {
            return new Schema(schemaProvider);
        }

        internal static void Add(string urlBase, ISchema schema)
        {
            Instances.GetOrAdd(urlBase, sp => schema);
        }

        internal static void ClearCache()
        {
            Instances.Clear();
        }
    }
}
