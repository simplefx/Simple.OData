using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;
using System.Diagnostics;
using Simple.Data.Extensions;
using Simple.OData;
using Simple.Data.OData.Helpers;

namespace Simple.Data.OData.Schema
{
    /// <summary>
    /// Represents an OData table and provides CRUD operations against it.
    /// </summary>
    public class Table
    {
        private readonly ProviderHelper _providerHelper;
        private readonly string _actualName;
        private readonly DatabaseSchema _databaseSchema;
        private readonly Lazy<ColumnCollection> _lazyColumns;
        private readonly Lazy<Key> _lazyPrimaryKey;

        // ReSharper disable InconsistentNaming
        private const string GET = "GET";
        private const string POST = "POST";
        private const string PUT = "PUT";
        private const string MERGE = "MERGE";
        private const string DELETE = "DELETE";
        // ReSharper restore InconsistentNaming

        public Table(string name, ProviderHelper providerHelper)
        {
            _providerHelper = providerHelper;
            _databaseSchema = DatabaseSchema.Get(_providerHelper);
            _actualName = name;
        }

        internal Table(string actualName, ProviderHelper providerHelper, DatabaseSchema databaseSchema)
        {
            _actualName = actualName;
            _providerHelper = providerHelper;
            _databaseSchema = databaseSchema;
            _lazyColumns = new Lazy<ColumnCollection>(GetColumns);
            _lazyPrimaryKey = new Lazy<Key>(GetPrimaryKey);
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

        private ColumnCollection GetColumns()
        {
            return new ColumnCollection(_databaseSchema.SchemaProvider.GetColumns(this));
        }

        private Key GetPrimaryKey()
        {
            return _databaseSchema.SchemaProvider.GetPrimaryKey(this);
        }

        public IEnumerable<IDictionary<string, object>> GetAllRows()
        {
            return Get(_actualName);
        }

        public IEnumerable<IDictionary<string, object>> QueryWithFilter(string filter)
        {
            return Get(_databaseSchema.FindTable(_actualName).ActualName + "?$filter=" + HttpUtility.UrlEncode(filter));
        }

        public IEnumerable<IDictionary<string, object>> QueryWithKeys(string keys)
        {
            return Get(_databaseSchema.FindTable(_actualName).ActualName + "(" + keys + ")");
        }

        private IEnumerable<IDictionary<string, object>> Get(string url)
        {
            IEnumerable<IDictionary<string, object>> result;
            var request = _providerHelper.CreateTableRequest(url, GET);

            using (var response = new RequestRunner().TryRequest(request))
            {
                Trace.WriteLine(response.StatusCode, "HttpResponse");

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    result = Enumerable.Empty<IDictionary<string, object>>();
                }
                else
                {
                    result = DataServicesHelper.GetData(response.GetResponseStream());
                }
            }

            return result;
        }
    }
}
