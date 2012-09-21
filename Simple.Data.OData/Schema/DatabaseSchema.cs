using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.OData.Schema;

namespace Simple.Data.OData.Schema
{
    public class DatabaseSchema
    {
        private static readonly ConcurrentDictionary<string, DatabaseSchema> Instances = new ConcurrentDictionary<string, DatabaseSchema>();

        private readonly string _urlBase;
        private readonly ISchemaProvider _schemaProvider;
        private readonly Lazy<TableCollection> _lazyTables;
        private readonly Lazy<FunctionCollection> _lazyFunctions;

        private DatabaseSchema(ISchemaProvider schemaProvider, string urlBase)
        {
            _lazyTables = new Lazy<TableCollection>(CreateTableCollection);
            _lazyFunctions = new Lazy<FunctionCollection>(CreateFunctionCollection);
            _schemaProvider = schemaProvider;
            _urlBase = urlBase;
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

        public bool HasTable(string tableName)
        {
            return _lazyTables.Value.Contains(tableName);
        }

        public IEnumerable<Function> Functions
        {
            get { return _lazyFunctions.Value.AsEnumerable(); }
        }

        public Function FindFunction(string functionName)
        {
            return _lazyFunctions.Value.Find(functionName);
        }

        public bool HasFunction(string functionName)
        {
            return _lazyFunctions.Value.Contains(functionName);
        }

        private TableCollection CreateTableCollection()
        {
            return new TableCollection(_schemaProvider.GetTables()
                .Select(table => new Table(table.ActualName, this)));
        }

        private FunctionCollection CreateFunctionCollection()
        {
            return new FunctionCollection(_schemaProvider.GetFunctions());
        }

        public static DatabaseSchema Get(string urlBase)
        {
            return Instances.GetOrAdd(urlBase,
                                      sp => new DatabaseSchema(new SchemaProvider(urlBase), urlBase));
        }

        public static void ClearCache()
        {
            Instances.Clear();
        }
    }
}
