using System.Linq;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public class SchemaTests : TestBase
    {
        public SchemaTests() 
            : base(null)
        {
        }

        [Fact]
        public async void MultipleSchemas_Marathon()
        {
            _client = CreateClient("Marathon.edmx");
            // TODO
            //var schema = await _client.GetSchemaAsync();
            //var table = schema.Tables;
        }

        [Fact]
        public async void MultipleSchemas_Insight()
        {
            _client = CreateClient("Insight.edmx");
            var schema = await _client.GetSchemaAsync();
            var table = schema.FindTable("Customers");
            Assert.NotNull(table);
            Assert.Equal(178, table.Columns.Count());
            var column = table.FindColumn("KeyCustomer");
            Assert.NotNull(column);
            var commandText = await _client.For("Customers").OrderBy("KeyCustomer").GetCommandTextAsync();
            Assert.Equal("Customers?$orderby=KeyCustomer", commandText);
            commandText = await _client.For("Products").OrderBy("KeyProduct").GetCommandTextAsync();
            Assert.Equal("Products?$orderby=KeyProduct", commandText);
        }
    }
}