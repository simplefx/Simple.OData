using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;
using System.Diagnostics;
using Simple.Data.Azure.Helpers;
using Simple.NExtLib.Xml;
using Simple.NExtLib.IO;
using System.Net.Cache;

namespace Simple.Data.Azure
{
    /// <summary>
    /// Represents an Azure table and provides CRUD operations against it.
    /// </summary>
    public class Table
    {
        private readonly AzureHelper _azureHelper;

        // ReSharper disable InconsistentNaming
        private const string GET = "GET";
        private const string POST = "POST";
        private const string PUT = "PUT";
        private const string MERGE = "MERGE";
        private const string DELETE = "DELETE";
        // ReSharper restore InconsistentNaming

        private readonly string _tableName;
        private readonly bool _autoCreate;

        public Table(string tableName, AzureHelper azureHelper) : this(tableName, IfTableDoesNotExist.ThrowAnException, azureHelper) { }

        public Table(string tableName, IfTableDoesNotExist doesNotExistAction, AzureHelper azureHelper)
        {
            _tableName = tableName;
            _azureHelper = azureHelper;
            _autoCreate = doesNotExistAction == IfTableDoesNotExist.CreateIt;
        }

        public IEnumerable<IDictionary<string, object>> GetAllRows()
        {
            return Get(_tableName);
        }

        public IDictionary<string, object> Get(string partitionKey, string rowKey)
        {
            return Get(BuildEntityUri(_tableName, partitionKey, rowKey)).SingleOrDefault();
        }

        public void Delete(IDictionary<string, object> row)
        {
            ThrowIfMissing(row, "PartitionKey", "RowKey");

            Delete(row["PartitionKey"].ToString(), row["RowKey"].ToString());
        }

        public void Delete(string partitionKey, string rowKey)
        {
            Delete(BuildEntityUri(_tableName, partitionKey, rowKey));
        }

        // TODO: Implement querying using LINQ, IQueryable<IDictionary<string, object>>
        public IEnumerable<IDictionary<string, object>> Query(string filter)
        {
            return Get(_tableName + "?$filter=" + HttpUtility.UrlEncode(filter));
        }

        public IDictionary<string, object> InsertRow(IDictionary<string, object> row)
        {
            ThrowIfMissing(row, "PartitionKey", "RowKey");

            var entry = DataServicesHelper.CreateDataElement(row);
            var request = _azureHelper.CreateTableRequest(_tableName, POST, entry.ToString());

            string text = string.Empty;

            if (_autoCreate)
            {
                try
                {
                    text = new RequestRunner().Request(request);
                }
                catch (TableServiceException ex)
                {
                    if (ex.Code == "TableNotFound")
                    {
                        Trace.WriteLine("Auto-creating table");
                        new TableService(_azureHelper).CreateTable(_tableName);
                        request = _azureHelper.CreateTableRequest(_tableName, POST, entry.ToString());

                        text = new RequestRunner().Request(request);
                    }
                }
            }
            else
            {
                text = new RequestRunner().Request(request);
            }

            return DataServicesHelper.GetData(text).First();
        }

        public void UpdateRow(IDictionary<string, object> row)
        {
            ThrowIfMissing(row, "PartitionKey", "RowKey");

            var dict = row.ToDictionary();
            var command = BuildEntityUri(_tableName, dict["PartitionKey"].ToString(), dict["RowKey"].ToString());

            WriteRowDataRequest(row, command, PUT);
        }

        public void MergeRow(string partitionKey, string rowKey, IDictionary<string, object> row)
        {
            ValidateKeyValue("partitionKey", partitionKey);
            ValidateKeyValue("rowKey", rowKey);

            var dict = row.ToDictionary();
            var command = BuildEntityUri(_tableName, partitionKey, rowKey);

            WriteRowDataRequest(row, command, MERGE);
        }

        public static IDictionary<string, object> NewRow(string partitionKey, string rowKey)
        {
            ValidateKeyValue("partitionKey", partitionKey);
            ValidateKeyValue("rowKey", rowKey);

            return new Dictionary<string, object>
            {
                { "PartitionKey", partitionKey },
                { "RowKey", rowKey }
            };
        }

        private static void ValidateKeyValue(string keyName, string value)
        {
            if (value == null) throw new ArgumentNullException(keyName);
            if (value.Contains("/")) throw new ArgumentException("Key values may not contain forward slashes", keyName);
            if (value.Contains("\\")) throw new ArgumentException("Key values may not contain backslashes", keyName);
            if (value.Contains("#")) throw new ArgumentException("Key values may not contain the hash/pound symbol", keyName);
            if (value.Contains("?")) throw new ArgumentException("Key values may not contain question marks", keyName);
        }

        private IEnumerable<IDictionary<string, object>> Get(string url)
        {
            IEnumerable<IDictionary<string, object>> result;
            var request = _azureHelper.CreateTableRequest(url, GET);

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

        private void Delete(string url)
        {
            var request = _azureHelper.CreateTableRequest(url, DELETE);

            new RequestRunner().TryRequest(request);
        }

        private void WriteRowDataRequest(IDictionary<string, object> row, string command, string verb)
        {
            var entry = DataServicesHelper.CreateDataElement(row);
            entry.Element(null, "id").Value = "http://" + _azureHelper.Account + ".table.core.windows.net/" + command;
            var request = _azureHelper.CreateTableRequest(command, verb, entry.ToString());
            request.Headers.Add(HttpRequestHeader.IfMatch, "*");
            request.Headers.Add("x-ms-version", "2009-09-19");

            using (new RequestRunner().TryRequest(request))
            {
                // No action, just disposing the response
            }
        }

        private static void ThrowIfMissing(IDictionary<string, object> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                if ((!row.ContainsKey(key)) || row[key] == null)
                {
                    throw new DataException("No or null " + key + "specified.");
                }
            }
        }

        private static string BuildEntityUri(string tableName, string partitionKey, string rowKey)
        {
            return string.Format(@"{0}(PartitionKey='{1}',RowKey='{2}')", tableName, partitionKey, rowKey);
        }
    }
}
