using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Data.OData.Schema;

namespace Simple.Data.OData.IntegrationTest
{
    using Xunit;

    public class SchemaTest : TestBase
    {
        private DatabaseSchema _schema;

        private DatabaseSchema GetSchema()
        {
            var adapter = _db.GetAdapter() as ODataTableAdapter;
            return adapter.GetSchema();
        }

        protected DatabaseSchema Schema
        {
            get { return (_schema ?? (_schema = GetSchema())); }
        }

        [Fact]
        public void GetTablesCount()
        {
            var tables = Schema.Tables;

            Assert.Equal(26, tables.Count());
        }

        [Fact]
        public void FindTable()
        {
            var table = Schema.FindTable("Customers");

            Assert.Equal(11, table.Columns.Count());
        }

        [Fact]
        public void FindColumn()
        {
            var column = Schema.FindTable("Employees").FindColumn("first_name");

            Assert.Equal("FirstName", column.ActualName);
        }

        [Fact]
        public void GetCompoundPrimaryKey()
        {
            var table = Schema.FindTable("OrderDetails");

            Assert.Equal("OrderID", table.PrimaryKey[0]);
            Assert.Equal("ProductID", table.PrimaryKey[1]);
        }
    }
}
