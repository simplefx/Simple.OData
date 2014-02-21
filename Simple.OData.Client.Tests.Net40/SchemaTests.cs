using System.Linq;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class SchemaTests : TestBase
    {
        [Fact]
        public void GetTablesCount()
        {
            var tables = _client.GetSchema().Tables;

            Assert.Equal(9, tables.Count());
        }

        [Fact]
        public void FindTable()
        {
            var table = _client.GetSchema().FindTable("Customers");

            Assert.NotNull(table);
        }

        [Fact]
        public void GetColumnsCount()
        {
            var columns = _client.GetSchema().FindTable("Employees").Columns;

            Assert.Equal(16, columns.Count());
        }

        [Fact]
        public void FindColumn()
        {
            var column = _client.GetSchema().FindTable("Employees").FindColumn("first_name");

            Assert.Equal("FirstName", column.ActualName);
        }

        [Fact]
        public void ColumnNullability()
        {
            var nonNullablecolumn = _client.GetSchema().FindTable("Employees").FindColumn("EmployeeID");
            var nullableColumn = _client.GetSchema().FindTable("Employees").FindColumn("ReportsTo");

            Assert.Equal(false, nonNullablecolumn.IsNullable);
            Assert.Equal(true, nullableColumn.IsNullable);
        }

        [Fact]
        public void GetAssociationsCount()
        {
            var associations = _client.GetSchema().FindTable("Employees").Associations;

            Assert.Equal(3, associations.Count());
        }

        [Fact]
        public void FindAssociation()
        {
            var association = _client.GetSchema().FindTable("Employees").FindAssociation("superior");

            Assert.Equal("Employees", association.ReferenceTableName);
            Assert.Equal("0..1", association.Multiplicity);
        }

        [Fact]
        public void GetCompoundPrimaryKey()
        {
            var table = _client.GetSchema().FindTable("OrderDetails");

            Assert.Equal("OrderID", table.PrimaryKey[0]);
            Assert.Equal("ProductID", table.PrimaryKey[1]);
        }

        [Fact]
        public void GetEntityTypesCount()
        {
            var entityTypes = _client.GetSchema().EntityTypes;

            Assert.Equal(11, entityTypes.Count());
        }

        [Fact]
        public void GetComplexTypesCount()
        {
            var complexTypes = _client.GetSchema().ComplexTypes;

            Assert.Equal(1, complexTypes.Count());
        }

        [Fact]
        public void GetSchemaAsString()
        {
            var schemaString = _client.GetSchemaAsString();

            Assert.Contains("Products", schemaString);
        }

        [Fact]
        public void ParseSchema()
        {
            var schemaString = _client.GetSchemaAsString();
            var schema = ODataClient.ParseSchemaString(schemaString);

            var table = schema.FindTable("OrderDetails");
            Assert.NotNull(table);
        }

        [Fact]
        public void CheckODataOrgAdventureWorksSchema()
        {
            var client = new ODataClient("http://services.odata.org/AdventureWorksV3/AdventureWorks.svc/");

            var table = client.GetSchema().FindTable("ProductCatalog");
            Assert.Equal("ID", table.PrimaryKey[0]);

            Assert.Equal(15, table.Columns.Count());

            var column = table.FindColumn("ThumbNailPhoto");
            Assert.Equal("Edm.Stream", column.PropertyType.Name);
            Assert.Equal(false, column.IsNullable);

            Assert.Equal(5, client.GetSchema().EntityTypes.Count());
            Assert.Equal(0, client.GetSchema().ComplexTypes.Count());
        }

        [Fact]
        public void CheckODataOrgNorthwindSchema()
        {
            var client = new ODataClient("http://services.odata.org/V2/Northwind/Northwind.svc/");

            var table = client.GetSchema().FindTable("Product");
            Assert.Equal("ProductID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Categories");
            Assert.Equal("Categories", association.ReferenceTableName);
            Assert.Equal("0..1", association.Multiplicity);

            table = client.GetSchema().FindTable("Employees");
            association = table.FindAssociation("Employees");
            Assert.Equal("Employees", association.ReferenceTableName);
            Assert.Equal("0..1", association.Multiplicity);

            Assert.Equal(26, client.GetSchema().EntityTypes.Count());
            Assert.Equal(0, client.GetSchema().ComplexTypes.Count());
        }

        [Fact]
        public void CheckODataOrgODataV2Schema()
        {
            var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");

            var table = client.GetSchema().FindTable("Product");
            Assert.Equal("ID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Category_Products");
            Assert.Equal("Categories", association.ReferenceTableName);
            Assert.Equal("0..1", association.Multiplicity);

            var function = client.GetSchema().FindFunction("GetProductsByRating");
            Assert.Equal(RestVerbs.GET, function.HttpMethod);
            Assert.Equal("rating", function.Parameters[0]);

            Assert.Equal(3, client.GetSchema().EntityTypes.Count());
            Assert.Equal(1, client.GetSchema().ComplexTypes.Count());
            Assert.Equal(5, client.GetSchema().ComplexTypes.First().Properties.Count());
        }

        [Fact]
        public void CheckODataOrgODataV3Schema()
        {
            var client = new ODataClient("http://services.odata.org/V3/OData/OData.svc/");

            var table = client.GetSchema().FindTable("Product");
            Assert.Equal("ID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Category_Products");
            Assert.Equal("Categories", association.ReferenceTableName);
            Assert.Equal("*", association.Multiplicity);

            var function = client.GetSchema().FindFunction("GetProductsByRating");
            Assert.Equal(RestVerbs.GET, function.HttpMethod);
            Assert.Equal("rating", function.Parameters[0]);

            Assert.Equal(10, client.GetSchema().EntityTypes.Count());
            Assert.Equal(1, client.GetSchema().ComplexTypes.Count());
            Assert.Equal(5, client.GetSchema().ComplexTypes.First().Properties.Count());
        }
    }
}
