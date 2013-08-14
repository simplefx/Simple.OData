using System.Linq;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class SchemaTests : TestBase
    {
        [Fact]
        public void GetTablesCount()
        {
            var tables = _client.Schema.Tables;

            Assert.Equal(9, tables.Count());
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
        public void ColumnNullability()
        {
            var nonNullablecolumn = _client.Schema.FindTable("Employees").FindColumn("EmployeeID");
            var nullableColumn = _client.Schema.FindTable("Employees").FindColumn("ReportsTo");

            Assert.Equal(false, nonNullablecolumn.IsNullable);
            Assert.Equal(true, nullableColumn.IsNullable);
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
        public void GetEntityTypesCount()
        {
            var entityTypes = _client.Schema.EntityTypes;

            Assert.Equal(11, entityTypes.Count());
        }

        [Fact]
        public void GetComplexTypesCount()
        {
            var complexTypes = _client.Schema.ComplexTypes;

            Assert.Equal(1, complexTypes.Count());
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

            var table = schema.FindTable("OrderDetails");
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

            Assert.Equal(26, client.Schema.EntityTypes.Count());
            Assert.Equal(0, client.Schema.ComplexTypes.Count());
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

            Assert.Equal(5, client.Schema.EntityTypes.Count());
            Assert.Equal(1, client.Schema.ComplexTypes.Count());
            Assert.Equal(5, client.Schema.ComplexTypes.First().Properties.Count());
        }

        [Fact]
        public void CheckODataOrgODataV3Schema()
        {
            var client = new ODataClient("http://services.odata.org/V3/OData/OData.svc/");

            var table = client.Schema.FindTable("Product");
            Assert.Equal("ID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Category_Products");
            Assert.Equal("Categories", association.ReferenceTableName);
            Assert.Equal("0..1", association.Multiplicity);

            var function = client.Schema.FindFunction("GetProductsByRating");
            Assert.Equal(RestVerbs.GET, function.HttpMethod);
            Assert.Equal("rating", function.Parameters[0]);

            Assert.Equal(5, client.Schema.EntityTypes.Count());
            Assert.Equal(1, client.Schema.ComplexTypes.Count());
            Assert.Equal(5, client.Schema.ComplexTypes.First().Properties.Count());
        }

        [Fact]
        public void CheckPluralsightComSchema()
        {
            var client = new ODataClient("http://pluralsight.com/odata/");

            var table = client.Schema.FindTable("Modules");
            Assert.Equal("Title", table.PrimaryKey[0]);

            Assert.NotNull(table.FindColumn("Author"));
            Assert.NotNull(table.FindColumn("Description"));

            var association = table.FindAssociation("Course");
            Assert.Equal("Courses", association.ReferenceTableName);
            Assert.Equal("*", association.Multiplicity);

            Assert.Equal(5, client.Schema.EntityTypes.Count());
            Assert.Equal(0, client.Schema.ComplexTypes.Count());
        }

        //[Fact]
        //public void RetrieveSchemaFromUrlWithoutFilename()
        //{
        //    var client = new ODataClient("http://vancouverdataservice.cloudapp.net/v1/impark");

        //    var schema = client.Schema;

        //    Assert.NotEmpty(schema.Tables);
        //}
    }
}
