using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace Simple.OData.Client.Tests
{
    public class NorthwindSchemaTests
    {
        private const string _serviceUrl = "http://services.odata.org/{0}/Northwind/Northwind.svc/";

        [Theory]
        [InlineData("V2", 26)]
        [InlineData("V3", 26)]
        [InlineData("V4", 26)]
        public async Task GetEntityTypesCount(string protocolVersion, int typeCount)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var entityTypes = (await client.GetSchemaAsync()).EntityTypes;

            Assert.Equal(typeCount, entityTypes.Count());
        }

        [Theory]
        [InlineData("V2", 0)]
        [InlineData("V3", 0)]
        [InlineData("V4", 0)]
        public async Task GetComplexTypesCount(string protocolVersion, int typeCount)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var complexTypes = (await client.GetSchemaAsync()).ComplexTypes;

            Assert.Equal(typeCount, complexTypes.Count());
        }

        [Theory]
        [InlineData("V2", 26)]
        [InlineData("V3", 26)]
        [InlineData("V4", 26)]
        public async Task GetTablesCount(string protocolVersion, int tablesCount)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var tables = (await client.GetSchemaAsync()).Tables;

            Assert.Equal(tablesCount, tables.Count());
        }

        [Theory]
        [InlineData("V2")]
        [InlineData("V3")]
        [InlineData("V4")]
        public async Task FindTable(string protocolVersion)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var table = (await client.GetSchemaAsync()).FindTable("Customers");

            Assert.NotNull(table);
        }

        [Theory]
        [InlineData("V2")]
        [InlineData("V3")]
        [InlineData("V4")]
        public async Task GetTableProperties(string protocolVersion)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var table = (await client.GetSchemaAsync()).FindTable("Customers");

            Assert.Equal("Customers", table.ActualName);
            Assert.Null(table.BaseTable);
            Assert.Equal("Customer", table.EntityType.Name);
            Assert.Equal(1, table.PrimaryKey.AsEnumerable().Count());
            //Assert.Equal(2, table.Associations.Count());
        }

        [Theory]
        [InlineData("V2")]
        [InlineData("V3")]
        [InlineData("V4")]
        public async Task GetColumnsCount(string protocolVersion)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var columns = (await client.GetSchemaAsync()).FindTable("Employees").Columns;

            Assert.Equal(18, columns.Count());
        }

        [Theory]
        [InlineData("V2")]
        [InlineData("V3")]
        [InlineData("V4")]
        public async Task FindColumn(string protocolVersion)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var column = (await client.GetSchemaAsync()).FindTable("Employees").FindColumn("first_name");

            Assert.NotNull(column);
        }

        [Theory]
        [InlineData("V2")]
        [InlineData("V3")]
        [InlineData("V4")]
        public async Task GetColumnProperties(string protocolVersion)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var column = (await client.GetSchemaAsync()).FindTable("Employees").FindColumn("first_name");

            Assert.Equal("FirstName", column.ActualName);
            Assert.Equal("Edm.String", column.PropertyType.Name);
        }

        [Theory]
        [InlineData("V2")]
        [InlineData("V3")]
        [InlineData("V4")]
        public async Task ColumnNullability(string protocolVersion)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var nonNullablecolumn = (await client.GetSchemaAsync()).FindTable("Employees").FindColumn("EmployeeID");
            var nullableColumn = (await client.GetSchemaAsync()).FindTable("Employees").FindColumn("ReportsTo");

            Assert.Equal(false, nonNullablecolumn.IsNullable);
            Assert.Equal(true, nullableColumn.IsNullable);
        }

        [Theory]
        [InlineData("V2")]
        [InlineData("V3")]
        [InlineData("V4")]
        public async Task GetScalarPrimaryKey(string protocolVersion)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var table = (await client.GetSchemaAsync()).FindTable("Product");
            Assert.Equal("ProductID", table.PrimaryKey[0]);
        }

        [Theory]
        [InlineData("V2")]
        [InlineData("V3")]
        [InlineData("V4")]
        public async Task GetCompoundPrimaryKey(string protocolVersion)
        {
            var client = new ODataClient(string.Format(_serviceUrl, protocolVersion));

            var table = (await client.GetSchemaAsync()).FindTable("OrderDetails");

            Assert.Equal("OrderID", table.PrimaryKey[0]);
            Assert.Equal("ProductID", table.PrimaryKey[1]);
        }
    }
}