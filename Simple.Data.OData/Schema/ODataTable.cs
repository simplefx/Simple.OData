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
using Simple.OData.Schema;

namespace Simple.Data.OData.Schema
{
    /// <summary>
    /// Represents an OData table and provides CRUD operations against it.
    /// </summary>
    public class ODataTable : Table
    {
        private readonly ProviderHelper _providerHelper;
        private readonly DatabaseSchema _databaseSchema;

        public ODataTable(string name, ProviderHelper providerHelper)
            : base(name)
        {
            _providerHelper = providerHelper;
            _databaseSchema = DatabaseSchema.Get(_providerHelper);
        }

        internal ODataTable(string name, ProviderHelper providerHelper, DatabaseSchema databaseSchema)
            : base(name)
        {
            _providerHelper = providerHelper;
            _databaseSchema = databaseSchema;
            _lazyColumns = new Lazy<ColumnCollection>(GetColumns);
            _lazyPrimaryKey = new Lazy<Key>(GetPrimaryKey);
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

        public int Delete(string keys)
        {
            string url = _databaseSchema.FindTable(_actualName).ActualName + "(" + keys + ")";
            var request = _providerHelper.CreateTableRequest(url, RestVerbs.DELETE);

            using (var response = new RequestRunner().TryRequest(request))
            {
                Trace.WriteLine(response.StatusCode, "HttpResponse");
                // TODO
                return response.StatusCode == HttpStatusCode.OK ? 1 : 0;
            }
        }

        private IEnumerable<IDictionary<string, object>> Get(string url)
        {
            IEnumerable<IDictionary<string, object>> result;
            var request = _providerHelper.CreateTableRequest(url, RestVerbs.GET);

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
