using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Simple.Data.Azure.Helpers;
using System.Net;

namespace Simple.Data.Azure
{
    public class TableService
    {
        private readonly AzureHelper _azureHelper;

        public TableService(AzureHelper azureHelper)
        {
            _azureHelper = azureHelper;
        }

        public IEnumerable<string> ListTables()
        {
            var request = _azureHelper.CreateTableRequest("tables", RestVerbs.GET);

            IEnumerable<string> list;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Trace.WriteLine(response.StatusCode, "HttpResponse");
                list = TableHelper.ReadTableList(response.GetResponseStream()).ToList();
            }

            return list;
        }

        public void CreateTable(string tableName)
        {
            var dict = new Dictionary<string, object> { { "TableName", tableName } };
            var data = DataServicesHelper.CreateDataElement(dict);

            DoRequest(data, "tables", RestVerbs.POST);
        }

        private void DoRequest(XElement element, string command, string method)
        {
            var request = _azureHelper.CreateTableRequest(command, method, element.ToString());

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Trace.WriteLine(response.StatusCode);
            }
        }
    }
}
