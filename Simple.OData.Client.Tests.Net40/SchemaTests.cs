using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class SchemaTests : TestBase
    {
        [Fact]
        public async Task GetTablesCount()
        {
            var tables = (await _client.GetSchemaAsync()).EntitySets;

            Assert.Equal(9, tables.Count());
        }

        [Fact]
        public async Task FindTable()
        {
            var table = (await _client.GetSchemaAsync()).FindEntitySet("Customers");

            Assert.NotNull(table);
        }

        //[Fact]
        //public async Task GetColumnsCount()
        //{
        //    var columns = (await _client.GetSchemaAsync()).FindEntitySet("Employees").Columns;

        //    Assert.Equal(16, columns.Count());
        //}

        //[Fact]
        //public async Task FindColumn()
        //{
        //    var column = (await _client.GetSchemaAsync()).FindEntitySet("Employees").FindColumn("first_name");

        //    Assert.Equal("FirstName", column.ActualName);
        //}

        //[Fact]
        //public async Task ColumnNullability()
        //{
        //    var nonNullablecolumn = (await _client.GetSchemaAsync()).FindEntitySet("Employees").FindColumn("EmployeeID");
        //    var nullableColumn = (await _client.GetSchemaAsync()).FindEntitySet("Employees").FindColumn("ReportsTo");

        //    Assert.Equal(false, nonNullablecolumn.IsNullable);
        //    Assert.Equal(true, nullableColumn.IsNullable);
        //}

        //[Fact]
        //public async Task GetAssociationsCount()
        //{
        //    var associations = (await _client.GetSchemaAsync()).FindEntitySet("Employees").Associations;

        //    Assert.Equal(3, associations.Count());
        //}

        //[Fact]
        //public async Task FindAssociation()
        //{
        //    var association = (await _client.GetSchemaAsync()).FindEntitySet("Employees").FindAssociation("superior");

        //    Assert.Equal("Employees", association.ReferenceTableName);
        //    Assert.Equal("0..1", association.Multiplicity);
        //}

        //[Fact]
        //public async Task GetCompoundPrimaryKey()
        //{
        //    var table = (await _client.GetSchemaAsync()).FindEntitySet("OrderDetails");

        //    Assert.Equal("OrderID", table.PrimaryKey[0]);
        //    Assert.Equal("ProductID", table.PrimaryKey[1]);
        //}

        [Fact]
        public async Task GetBaseType()
        {
            var entityType = (await _client.GetSchemaAsync()).EntityTypes.Single(x => x.Name == "Ships");

            Assert.Equal("Transport", entityType.BaseType.Name);
        }

        [Fact]
        public async Task GetEntityTypesCount()
        {
            var entityTypes = (await _client.GetSchemaAsync()).EntityTypes;

            Assert.Equal(11, entityTypes.Count());
        }

        [Fact]
        public async Task GetComplexTypesCount()
        {
            var complexTypes = (await _client.GetSchemaAsync()).ComplexTypes;

            Assert.Equal(1, complexTypes.Count());
        }

        [Fact]
        public async Task GetSchemaAsString()
        {
            var schemaString = await _client.GetSchemaAsStringAsync();

            Assert.Contains("Products", schemaString);
        }

        [Fact]
        public async Task ParseSchema()
        {
            var schemaString = await _client.GetSchemaAsStringAsync();
            var schema = ODataClient.ParseSchemaString(schemaString);

            var table = schema.FindEntitySet("OrderDetails");
            Assert.NotNull(table);
        }
    }
}
