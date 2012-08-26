using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Simple.Data.Extensions;

namespace Simple.Data.OData.Schema
{
    /// <summary>
    /// Represents an OData table and provides CRUD operations against it.
    /// </summary>
    public class Table
    {
        protected readonly string _actualName;
        protected Lazy<ColumnCollection> _lazyColumns;
        protected Lazy<AssociationCollection> _lazyAssociations;
        protected Lazy<Key> _lazyPrimaryKey;

        public Table(string name)
        {
            _actualName = name;
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
    }
}
