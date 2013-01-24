using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    /// <summary>
    /// Represents an OData table and provides CRUD operations against it.
    /// </summary>
    public class Table
    {
        private readonly Schema _schema;
        private readonly string _actualName;
        private Lazy<ColumnCollection> _lazyColumns;
        private Lazy<AssociationCollection> _lazyAssociations;
        private Lazy<Key> _lazyPrimaryKey;

        internal Table(string name, Schema schema)
        {
            _actualName = name;
            _schema = schema;
            _lazyColumns = new Lazy<ColumnCollection>(GetColumns);
            _lazyAssociations = new Lazy<AssociationCollection>(GetAssociations);
            _lazyPrimaryKey = new Lazy<Key>(GetPrimaryKey);
        }

        public override string ToString()
        {
            return _actualName;
        }

        internal string HomogenizedName
        {
            get { return _actualName.Homogenize(); }
        }

        public string ActualName
        {
            get { return _actualName; }
        }

        public IEnumerable<Column> Columns
        {
            get { return _lazyColumns.Value.AsEnumerable(); }
        }

        public Column FindColumn(string columnName)
        {
            var columns = _lazyColumns.Value;
            try
            {
                return columns.Find(columnName);
            }
            catch (UnresolvableObjectException ex)
            {
                string qualifiedName = _actualName + "." + ex.ObjectName;
                throw new UnresolvableObjectException(qualifiedName, string.Format("Column {0} not found", qualifiedName), ex);
            }
        }

        public bool HasColumn(string columnName)
        {
            return _lazyColumns.Value.Contains(columnName);
        }

        public IEnumerable<Association> Associations
        {
            get { return _lazyAssociations.Value.AsEnumerable(); }
        }

        public Association FindAssociation(string associationName)
        {
            var associations = _lazyAssociations.Value;
            try
            {
                return associations.Find(associationName);
            }
            catch (UnresolvableObjectException ex)
            {
                string qualifiedName = _actualName + "." + ex.ObjectName;
                throw new UnresolvableObjectException(qualifiedName, string.Format("Association {0} not found", qualifiedName), ex);
            }
        }

        public bool HasAssociation(string associationName)
        {
            return _lazyAssociations.Value.Contains(associationName);
        }

        public Key PrimaryKey
        {
            get { return _lazyPrimaryKey.Value; }
        }

        public IDictionary<string, object> GetKey(string tableName, IDictionary<string, object> record)
        {
            var keyNames = GetKeyNames();
            return record.Where(x => keyNames.Contains(x.Key)).ToIDictionary();
        }

        public IList<string> GetKeyNames()
        {
            return _schema.FindTable(_actualName).PrimaryKey.AsEnumerable().ToList();
        }

        private ColumnCollection GetColumns()
        {
            return new ColumnCollection(_schema.SchemaProvider.GetColumns(this));
        }

        private AssociationCollection GetAssociations()
        {
            return new AssociationCollection(_schema.SchemaProvider.GetAssociations(this));
        }

        private Key GetPrimaryKey()
        {
            return _schema.SchemaProvider.GetPrimaryKey(this);
        }
    }
}
