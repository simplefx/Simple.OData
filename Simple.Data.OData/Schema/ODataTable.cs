using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.OData;
using Simple.Data.OData.Schema;

namespace Simple.Data.OData.Schema
{
    /// <summary>
    /// Represents an OData table and provides CRUD operations against it.
    /// </summary>
    public class ODataTable : Table
    {
        private readonly DatabaseSchema _databaseSchema;

        internal ODataTable(string name, DatabaseSchema databaseSchema)
            : base(name)
        {
            _databaseSchema = databaseSchema;
            _lazyColumns = new Lazy<ColumnCollection>(GetColumns);
            _lazyAssociations = new Lazy<AssociationCollection>(GetAssociations);
            _lazyPrimaryKey = new Lazy<Key>(GetPrimaryKey);
        }

        private ColumnCollection GetColumns()
        {
            return new ColumnCollection(_databaseSchema.SchemaProvider.GetColumns(this));
        }

        private AssociationCollection GetAssociations()
        {
            return new AssociationCollection(_databaseSchema.SchemaProvider.GetAssociations(this));
        }

        private Key GetPrimaryKey()
        {
            return _databaseSchema.SchemaProvider.GetPrimaryKey(this);
        }

        public IDictionary<string, object> GetKey(string tableName, IDictionary<string, object> record)
        {
            var keyNames = GetKeyNames();
            return record.Where(x => keyNames.Contains(x.Key)).ToIDictionary();
        }

        public IList<string> GetKeyNames()
        {
            return _databaseSchema.FindTable(_actualName).PrimaryKey.AsEnumerable().ToList();
        }
    }
}
