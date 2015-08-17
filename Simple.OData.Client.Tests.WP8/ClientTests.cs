using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Simple.OData.Client.Tests
{
    using Entry = System.Collections.Generic.Dictionary<string, object>;

    [TestClass]
    public class ClientTests
    {
        private ODataClient _client;

        [TestInitialize]
        public void TestInitialize()
        {
            _client = new ODataClient("http://services.odata.org/V3/OData/OData.svc/");
        }

        [TestMethod]
        public async Task FindEntries()
        {
            var products = await _client.FindEntriesAsync("Products");
            Assert.IsTrue(products.Any());
        }

        [TestMethod]
        public async Task FindEntriesNonExisting()
        {
            var products = await _client.FindEntriesAsync("Products?$filter=ID eq -1");
            Assert.IsTrue(!products.Any());
        }

        private class Product
        {
            public int ID { get; set; }
        }

        [TestMethod]
        public async Task FindEntriesWithPluralizer()
        {
            var products = await _client.For<Product>().FindEntriesAsync();
            Assert.IsTrue(products.Any());
        }

        [TestMethod]
        public async Task GetEntryExisting()
        {
            var product = await _client.GetEntryAsync("Products", new Entry() { { "ID", 1 } });
            Assert.AreEqual(1, product["ID"]);
        }

        [TestMethod]
        public async Task GetEntryNonExisting()
        {
            await AssertThrowsAsync<WebRequestException>(async () => await _client.GetEntryAsync("Products", new Entry() { { "ID", -1 } }));
        }

        public async static Task<T> AssertThrowsAsync<T>(Func<Task> testCode) where T : Exception
        {
            try
            {
                await testCode();
                Assert.Fail("Expected exception: {0}", typeof(T));
            }
            catch (T exception)
            {
                return exception;
            }
            return null;
        }
    }
}