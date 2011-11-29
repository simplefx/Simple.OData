using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.OData.Helpers;
using Simple.OData.Schema;

namespace Simple.Data.OData.Schema
{
    public class DatabaseSchema
    {
        private static readonly ConcurrentDictionary<string, DatabaseSchema> Instances = new ConcurrentDictionary<string, DatabaseSchema>();

        private readonly ProviderHelper _providerHelper;
        private readonly ISchemaProvider _schemaProvider;
        private readonly Lazy<TableCollection> _lazyTables;

        private DatabaseSchema(ISchemaProvider schemaProvider, ProviderHelper providerHelper)
        {
            _lazyTables = new Lazy<TableCollection>(CreateTableCollection);
            _schemaProvider = schemaProvider;
            _providerHelper = providerHelper;
        }

        public ProviderHelper ProviderHelper
        {
            get { return _providerHelper; }
        }

        public ISchemaProvider SchemaProvider
        {
            get { return _schemaProvider; }
        }

        public bool IsAvailable
        {
            get { return _schemaProvider != null; }
        }

        public IEnumerable<Table> Tables
        {
            get { return _lazyTables.Value.AsEnumerable(); }
        }

        public Table FindTable(string tableName)
        {
            return _lazyTables.Value.Find(tableName);
        }

        private TableCollection CreateTableCollection()
        {
            return new TableCollection(_schemaProvider.GetTables()
                .Select(table => new ODataTable(table.ActualName, _providerHelper, this)));
        }

        public static DatabaseSchema Get(ProviderHelper providerHelper)
        {
            return Instances.GetOrAdd(providerHelper.UrlBase,
                                      sp => new DatabaseSchema(new SchemaProvider(providerHelper), providerHelper));
        }

        public static void ClearCache()
        {
            Instances.Clear();
        }
    }
}
