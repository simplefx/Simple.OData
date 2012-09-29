using System.Linq;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class SchemaTest : TestBase
    {
        [Fact]
        public void GetTablesCount()
        {
            var tables = _client.Schema.Tables;

            Assert.Equal(8, tables.Count());
        }

        [Fact]
        public void FindTable()
        {
            var table = _client.Schema.FindTable("Customers");

            Assert.NotNull(table);
        }

        [Fact]
        public void GetColumnsCount()
        {
            var columns = _client.Schema.FindTable("Employees").Columns;

            Assert.Equal(16, columns.Count());
        }

        [Fact]
        public void FindColumn()
        {
            var column = _client.Schema.FindTable("Employees").FindColumn("first_name");

            Assert.Equal("FirstName", column.ActualName);
        }

        [Fact]
        public void GetAssociationsCount()
        {
            var associations = _client.Schema.FindTable("Employees").Associations;

            Assert.Equal(3, associations.Count());
        }

        [Fact]
        public void FindAssociation()
        {
            var association = _client.Schema.FindTable("Employees").FindAssociation("superior");

            Assert.Equal("Employees", association.ReferenceTableName);
            Assert.Equal("0..1", association.Multiplicity);
        }

        [Fact]
        public void GetCompoundPrimaryKey()
        {
            var table = _client.Schema.FindTable("OrderDetails");

            Assert.Equal("OrderID", table.PrimaryKey[0]);
            Assert.Equal("ProductID", table.PrimaryKey[1]);
        }

        [Fact]
        public void GetSchemaAsString()
        {
            var schemaString = _client.SchemaAsString;

            Assert.Contains("Products", schemaString);
        }

        [Fact]
        public void ParseSchema()
        {
            var schemaString = _client.SchemaAsString;
            var schema = ODataClient.ParseSchemaString(schemaString);

            var table = _client.Schema.FindTable("OrderDetails");
            Assert.NotNull(table);
        }

        [Fact]
        public void CheckODataOrgNorthwindSchema()
        {
            var client = new ODataClient("http://services.odata.org/Northwind/Northwind.svc/");

            var table = client.Schema.FindTable("Product");
            Assert.Equal("ProductID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Categories");
            Assert.Equal("Categories", association.ReferenceTableName);
            Assert.Equal("0..1", association.Multiplicity);

            table = client.Schema.FindTable("Employees");
            association = table.FindAssociation("Employees");
            Assert.Equal("Employees", association.ReferenceTableName);
            Assert.Equal("0..1", association.Multiplicity);
        }

        [Fact]
        public void CheckODataOrgODataSchema()
        {
            var client = new ODataClient("http://services.odata.org/OData/OData.svc/");

            var table = client.Schema.FindTable("Product");
            Assert.Equal("ID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Category_Products");
            Assert.Equal("Categories", association.ReferenceTableName);
            Assert.Equal("0..1", association.Multiplicity);

            var function = client.Schema.FindFunction("GetProductsByRating");
            Assert.Equal(RestVerbs.GET, function.HttpMethod);
            Assert.Equal("rating", function.Parameters[0]);
        }
    }
}
