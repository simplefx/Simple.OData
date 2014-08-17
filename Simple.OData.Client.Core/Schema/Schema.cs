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

        private readonly SchemaProvider _schemaProvider;
        private readonly Func<Task<string>> _resolveMetadataAsync;
        private Func<EdmSchema> _createEdmSchema;
        private Func<ProviderMetadata> _createProviderMetadata;
        private string _metadataString;

        private Lazy<EdmSchema> _lazyMetadata;
        private Lazy<ProviderMetadata> _lazyProviderMetadata;
        private Lazy<TableCollection> _lazyTables;
        private Lazy<List<EdmEntityType>> _lazyEntityTypes;
        private Lazy<List<EdmComplexType>> _lazyComplexTypes;

        private Schema(string metadataString, Func<Task<string>> resolveMedatataAsync)
        {
            ResetCache();
            _model = new Model(this);

            _metadataString = metadataString;
            _resolveMetadataAsync = resolveMedatataAsync;

            if (_resolveMetadataAsync == null)
            {
                _createEdmSchema = () => ResponseReader.GetSchema(_metadataString);
            }
        }

        private Schema(SchemaProvider schemaProvider)
        {
            ResetCache();
            _model = new Model(this);

            _schemaProvider = schemaProvider;
        }

        internal void ResetCache()
        {
            _metadataString = null;

            _lazyMetadata = new Lazy<EdmSchema>(CreateEdmSchema);
            _lazyProviderMetadata = new Lazy<ProviderMetadata>(CreateProviderMetadata);
            _lazyTables = new Lazy<TableCollection>(CreateTableCollection);
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
                if (_schemaProvider != null)
                {
                    var response = await _schemaProvider.SendSchemaRequestAsync(cancellationToken);
                    _metadataString = await _schemaProvider.GetSchemaAsStringAsync(response);
                    var providerMetadata = await _schemaProvider.GetMetadataAsync(response);
                    _createProviderMetadata = () => providerMetadata;
                    var metadata = await _schemaProvider.GetSchemaAsync(providerMetadata);
                    _createEdmSchema = () => metadata;
                }
                else
                {
                    _metadataString = await _resolveMetadataAsync();
                    _createEdmSchema = () => ResponseReader.GetSchema(_metadataString);
                    // TODO
                }
            }

            _lazyMetadata = new Lazy<EdmSchema>(CreateEdmSchema);
            _lazyProviderMetadata = new Lazy<ProviderMetadata>(CreateProviderMetadata);
            return this;
        }

        public EdmSchema Metadata
        {
            get { return _lazyMetadata.Value; }
        }

        public ProviderMetadata ProviderMetadata
        {
            get { return _lazyProviderMetadata.Value; }
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

        private EdmSchema CreateEdmSchema()
        {
            return _createEdmSchema();
        }

        private ProviderMetadata CreateProviderMetadata()
        {
            return _createProviderMetadata();
        }

        private TableCollection CreateTableCollection()
        {
            return new TableCollection(_model.GetTables()
                .Select(table => new Table(table.ActualName, table.EntityType, null, this)));
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
            return Instances.GetOrAdd(urlBase, new Schema(new SchemaProvider(urlBase, credentials)));
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
