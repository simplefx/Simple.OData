using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;
using System.Diagnostics;
using Simple.OData;
using Simple.Data.OData.Helpers;

namespace Simple.Data.OData
{
    /// <summary>
    /// Represents an OData table and provides CRUD operations against it.
    /// </summary>
    public class Table
    {
        private readonly ODataHelper _odataHelper;

        // ReSharper disable InconsistentNaming
        private const string GET = "GET";
        private const string POST = "POST";
        private const string PUT = "PUT";
        private const string MERGE = "MERGE";
        private const string DELETE = "DELETE";
        // ReSharper restore InconsistentNaming

        private readonly string _tableName;

        public Table(string tableName, ODataHelper odataHelper)
        {
            _tableName = tableName;
            _odataHelper = odataHelper;
        }

        public IEnumerable<IDictionary<string, object>> GetAllRows()
        {
            return Get(_tableName);
        }

        // TODO: Implement querying using LINQ, IQueryable<IDictionary<string, object>>
        public IEnumerable<IDictionary<string, object>> Query(string filter)
        {
            return Get(_tableName + "?$filter=" + HttpUtility.UrlEncode(filter));
        }

        private IEnumerable<IDictionary<string, object>> Get(string url)
        {
            IEnumerable<IDictionary<string, object>> result;
            var request = _odataHelper.CreateTableRequest(url, GET);

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
