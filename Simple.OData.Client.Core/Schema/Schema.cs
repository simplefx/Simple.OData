using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client
{
    class Schema : ISchema
    {
        private static readonly SimpleDictionary<string, ISchema> Instances = new SimpleDictionary<string, ISchema>();

        private readonly Model _model;

        private readonly Func<Task<string>> _resolveMetadataAsync;
        private string _metadataString;

        private Lazy<EdmSchema> _lazyMetadata;
        private Lazy<TableCollection> _lazyTables;
        private Lazy<FunctionCollection> _lazyFunctions;
        private Lazy<List<EdmEntityType>> _lazyEntityTypes;
        private Lazy<List<EdmComplexType>> _lazyComplexTypes;

        private Schema(string metadataString, Func<Task<string>> resolveMedatataAsync)
        {
            _model = new Model(this);

            ResetCache();

            _metadataString = metadataString;
            _resolveMetadataAsync = resolveMedatataAsync;
        }

        internal void ResetCache()
        {
            _metadataString = null;

            _lazyMetadata = new Lazy<EdmSchema>(GetCreateSchemaFunc());
            _lazyTables = new Lazy<TableCollection>(CreateTableCollection);
            _lazyFunctions = new Lazy<FunctionCollection>(CreateFunctionCollection);
            _lazyEntityTypes = new Lazy<List<EdmEntityType>>(CreateEntityTypeCollection);
            _lazyComplexTypes = new Lazy<List<EdmComplexType>>(CreateComplexTypeCollection);
        }

        internal Model Model
        {
            get { return _model; }
        }

        public async Task<ISchema> ResolveAsync(CancellationToken cancellationToken)
        {
            if (_metadataString == null)
            {
                _metadataString = await _resolveMetadataAsync();
                _lazyMetadata = new Lazy<EdmSchema>(GetCreateSchemaFunc());
            }
            return this;
        }

        public EdmSchema Metadata
        {
            get { return _lazyMetadata.Value; }
        }

        public string MetadataAsString
        {
            get { return _metadataString; }
        }

        public string TypesNamespace
        {
            get { return string.Empty; }
        }

        public string ContainersNamespace
        {
            get { return string.Empty; }
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

        private Func<EdmSchema> GetCreateSchemaFunc()
        {
            return () => ResponseReader.GetSchema(_metadataString);
        }

        private TableCollection CreateTableCollection()
        {
            return new TableCollection(_model.GetTables()
                .Select(table => new Table(table.ActualName, table.EntityType, null, this)));
        }

        private FunctionCollection CreateFunctionCollection()
        {
            return new FunctionCollection(_model.GetFunctions());
        }

        private List<EdmEntityType> CreateEntityTypeCollection()
        {
            return new List<EdmEntityType>(_model.GetEntityTypes());
        }

        private List<EdmComplexType> CreateComplexTypeCollection()
        {
            return new List<EdmComplexType>(_model.GetComplexTypes());
        }

        internal static ISchema FromUrl(string urlBase, ICredentials credentials = null)
        {
            return Instances.GetOrAdd(urlBase, new Schema(null, async () => await ODataClient.GetSchemaAsStringAsync(urlBase, credentials)));
        }

        internal static ISchema FromMetadata(string metadataString)
        {
            return new Schema(metadataString, null);
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
