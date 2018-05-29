using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests.FluentApi
{
    public class DeleteTypedTests : TestBase
    {
        [Fact]
        public async Task DeleteByKey()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var product = await client
                .For<Product>()
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntryAsync();

            await client
                .For<Product>()
                .Key(product.ProductID)
                .DeleteEntryAsync();

            product = await client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .FindEntryAsync();

            Assert.Null(product);
        }

        [Fact]
        public async Task DeleteByFilter()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var product = await client
                .For<Product>()
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntryAsync();

            await client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .DeleteEntryAsync();

            product = await client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .FindEntryAsync();

            Assert.Null(product);
        }

        [Fact]
        public async Task DeleteByObjectAsKey()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var product = await client
                .For<Product>()
                .Set(new { ProductName = "Test1", UnitPrice = 18m })
                .InsertEntryAsync();

            await client
                .For<Product>()
                .Key(product)
                .DeleteEntryAsync();

            product = await client
                .For<Product>()
                .Filter(x => x.ProductName == "Test1")
                .FindEntryAsync();

            Assert.Null(product);
        }

        [Fact]
        public async Task DeleteDerived()
        {
            var client = new ODataClient(CreateDefaultSettings().WithHttpMock());
            var ship = await client
                .For<Transport>()
                .As<Ship>()
                .Set(new Ship { ShipName = "Test1" })
                .InsertEntryAsync();

            await client
                .For<Transport>()
                .As<Ship>()
                .Key(ship.TransportID)
                .DeleteEntryAsync();

            ship = await client
                .For<Transport>()
                .As<Ship>()
                .Filter(x => x.ShipName == "Test1")
                .FindEntryAsync();

            Assert.Null(ship);
        }
    }
}
