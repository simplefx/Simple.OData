using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.OData.Schema;

namespace Simple.Data.OData.IntegrationTest
{
    using Xunit;

    public class SchemaTest
    {
        private const string _northwindUrl = "http://services.odata.org/Northwind/Northwind.svc/";

        [Fact]
        public void ShouldGetCorrectTableCount()
        {
            var db = Database.Opener.Open(_northwindUrl);
            var adapter = db.GetAdapter() as ODataTableAdapter;

            var schema = adapter.GetSchema();
            var tables = schema.Tables;

            Assert.Equal(26, tables.Count());
        }

        [Fact]
        public void ShouldFindTable()
        {
            var db = Database.Opener.Open(_northwindUrl);
            var adapter = db.GetAdapter() as ODataTableAdapter;

            var schema = adapter.GetSchema();
            var table = schema.FindTable("Customers");

            Assert.Equal(23, table.Columns.Count());
        }
    }
}
