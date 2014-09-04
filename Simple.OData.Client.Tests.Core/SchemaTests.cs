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

        //[Fact]
        //public async void MultipleSchemas_Marathon()
        //{
        //    _client = CreateClient("Marathon.edmx");
        //    var Session = await _client.GetMetadataAsync();
        //    var table = Session.FindEntitySet("Marathons");
        //    Assert.NotNull(table);
        //    //Assert.Equal(8, EntitySet.Columns.Count());
        //}

        //[Fact]
        //public async void MultipleSchemas_Insight()
        //{
        //    _client = CreateClient("Insight.edmx");
        //    var Session = await _client.GetMetadataAsync();
        //    var table = Session.FindEntitySet("Customers");
        //    Assert.NotNull(table);
        //    //Assert.Equal(178, EntitySet.Columns.Count());
        //    //var column = EntitySet.FindColumn("KeyCustomer");
        //    //Assert.NotNull(column);
        //    var commandText = await _client.For("Customers").OrderBy("KeyCustomer").GetCommandTextAsync();
        //    Assert.Equal("Customers?$orderby=KeyCustomer", commandText);
        //    commandText = await _client.For("Products").OrderBy("KeyProduct").GetCommandTextAsync();
        //    Assert.Equal("Products?$orderby=KeyProduct", commandText);
        //}
    }
}