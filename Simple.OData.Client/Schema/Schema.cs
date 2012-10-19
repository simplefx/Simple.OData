using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    class Schema : ISchema
    {
        private static readonly ConcurrentDictionary<string, Schema> Instances = new ConcurrentDictionary<string, Schema>();

        private readonly ISchemaProvider _schemaProvider;
        private readonly Lazy<TableCollection> _lazyTables;
        private readonly Lazy<FunctionCollection> _lazyFunctions;

        private Schema(ISchemaProvider schemaProvider)
        {
            _lazyTables = new Lazy<TableCollection>(CreateTableCollection);
            _lazyFunctions = new Lazy<FunctionCollection>(CreateFunctionCollection);
            _schemaProvider = schemaProvider;
        }

        public ISchemaProvider SchemaProvider
        {
            get { return _schemaProvider; }
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

        internal static Schema Get(string urlBase)
        {
            return Instances.GetOrAdd(urlBase,
                                      sp => new Schema(Client.SchemaProvider.FromUrl(urlBase)));
        }

        internal static Schema Get(SchemaProvider schemaProvider)
        {
            return new Schema(schemaProvider);
        }

        internal static void ClearCache()
        {
            Instances.Clear();
        }
    }
}
