using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Diagnostics;
using Simple.Data;
using Simple.Data.Extensions;

namespace Simple.OData.Schema
{
    /// <summary>
    /// Represents an OData table and provides CRUD operations against it.
    /// </summary>
    public class Table
    {
        protected readonly string _actualName;
        protected Lazy<ColumnCollection> _lazyColumns;
        protected Lazy<Key> _lazyPrimaryKey;

        // ReSharper disable InconsistentNaming
        protected const string GET = "GET";
        protected const string POST = "POST";
        protected const string PUT = "PUT";
        protected const string MERGE = "MERGE";
        protected const string DELETE = "DELETE";
        // ReSharper restore InconsistentNaming

        public Table(string name)
        {
            _actualName = name;
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
                throw new UnresolvableObjectException(_actualName + "." + ex.ObjectName, "Column not found", ex);
            }
        }

        public bool HasColumn(string columnName)
        {
            return _lazyColumns.Value.Contains(columnName);
        }

        public Key PrimaryKey
        {
            get { return _lazyPrimaryKey.Value; }
        }
    }
}
