using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Simple.Data.Azure.Schema;
using Simple.OData;

namespace Simple.Data.Azure
{
    public class TableService
    {
        private readonly RequestBuilder _requestBuilder;

        public TableService(RequestBuilder requestBuilder)
        {
            _requestBuilder = requestBuilder;
        }

        public IEnumerable<string> ListTables()
        {
            var request = _requestBuilder.CreateTableRequest("Tables", RestVerbs.GET);

            IEnumerable<string> list;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                list = TableHelper.ReadTableList(response.GetResponseStream()).ToList();
            }

            return list;
        }

        public void CreateTable(string tableName)
        {
            var dict = new Dictionary<string, object> { { "TableName", tableName } };
            var data = DataServicesHelper.CreateDataElement(dict);

            DoRequest(data, "Tables", RestVerbs.POST);
        }

        private void DoRequest(XElement element, string command, string method)
        {
            var request = _requestBuilder.CreateTableRequest(command, method, element.ToString());

            using (var response = (HttpWebResponse)request.GetResponse())
            {
            }
        }
    }
}
