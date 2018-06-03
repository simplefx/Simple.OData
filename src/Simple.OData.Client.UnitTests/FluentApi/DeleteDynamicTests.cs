using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests.FluentApi
{
    public class DeleteDynamicTests : TestBase
    {
        [Fact]
        public async Task DeleteByKey()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var product = await client
                .For(x.Products)
                .Set(x.ProductName = "Test1", x.UnitPrice = 18m)
                .InsertEntryAsync();

            await client
                .For(x.Products)
                .Key(product.ProductID)
                .DeleteEntryAsync();

            product = await client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .FindEntryAsync();

            Assert.Null(product);
        }

        [Fact]
        public async Task DeleteByFilter()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var product = await client
                .For(x.Products)
                .Set(x.ProductName = "Test1", x.UnitPrice = 18m)
                .InsertEntryAsync();

            await client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .DeleteEntryAsync();

            product = await client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .FindEntryAsync();

            Assert.Null(product);
        }

        [Fact]
        public async Task DeleteByObjectAsKey()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var product = await client
                .For(x.Products)
                .Set(x.ProductName = "Test1", x.UnitPrice = 18m)
                .InsertEntryAsync();

            await client
                .For(x.Products)
                .Key(product)
                .DeleteEntryAsync();

            product = await client
                .For(x.Products)
                .Filter(x.ProductName == "Test1")
                .FindEntryAsync();

            Assert.Null(product);
        }

        [Fact]
        public async Task DeleteDerived()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var x = ODataDynamic.Expression;
            var ship = await client
                .For(x.Transport)
                .As(x.Ship)
                .Set(x.ShipName = "Test1")
                .InsertEntryAsync();

            await client
                .For(x.Transport)
                .As(x.Ship)
                .Key(ship.TransportID)
                .DeleteEntryAsync();

            ship = await client
                .For(x.Transport)
                .As(x.Ship)
                .Filter(x.ShipName == "Test1")
                .FindEntryAsync();

            Assert.Null(ship);
        }
    }
}
