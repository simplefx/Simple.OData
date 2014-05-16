using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public class WebApiTests : IDisposable
    {
        private string _serviceUri;
        private IODataClient _client;
        private const bool _useBasicAuthentication = true;
        private const string _user = "tester";
        private const string _password = "tester123";

        public WebApiTests()
        {
            var settings = new ODataClientSettings();
            settings.UrlBase = "http://" + "WEBAPI-PRODUCTS/ProductService/odata";
            if (_useBasicAuthentication)
            {
                settings.Credentials = new NetworkCredential(_user, _password);
            }
            _client = new ODataClient(settings);
        }

        public async void Dispose()
        {
            if (_client != null)
            {
                var products = await _client.FindEntriesAsync("Products");
                foreach (var product in products)
                {
                    if (product["Name"].ToString().StartsWith("Test"))
                        await _client.DeleteEntryAsync("Products", product);
                }

                var workTaskModels = await _client.FindEntriesAsync("WorkTaskModels");
                foreach (var workTaskModel in workTaskModels)
                {
                    if (workTaskModel["Code"].ToString().StartsWith("Test"))
                        await _client.DeleteEntryAsync("workTaskModels", workTaskModel);
                }
            }
        }

        [Fact]
        public async Task GetProductsCount()
        {
            var products = await _client
                .For("Products")
                .FindEntriesAsync();

            Assert.Equal(5, products.Count());
        }

        [Fact]
        public async Task InsertProduct()
        {
            var product = await _client
                .For("Products")
                .Set(new Entry() { { "Name", "Test1" }, { "Price", 18m } })
                .InsertEntryAsync();

            Assert.Equal("Test1", product["Name"]);
        }

        [Fact]
        public async Task UpdateProduct()
        {
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntryAsync();

            product = await _client
                .For("Products")
                .Key(product["ID"])
                .Set(new { Price = 123m })
                .UpdateEntryAsync();

            Assert.Equal(123m, product["Price"]);
        }

        [Fact]
        public async Task DeleteProduct()
        {
            var product = await _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntryAsync();

            await _client
                .For("Products")
                .Key(product["ID"])
                .DeleteEntryAsync();

            product = await _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .FindEntryAsync();

            Assert.Null(product);
        }

        [Fact]
        public async Task InsertWorkTaskModel()
        {
            var workTaskModel = await _client
                .For("WorkTaskModels")
                .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0f},{"Longitude", 2.0f}}  },
                })
                .InsertEntryAsync();

            Assert.Equal("Test1", workTaskModel["Code"]);
        }

        [Fact]
        public async Task UpdateWorkTaskModel()
        {
            var workTaskModel = await _client
                .For("WorkTaskModels")
                .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0f},{"Longitude", 2.0f}}  },
                })
                .InsertEntryAsync();

            workTaskModel = await _client
                .For("WorkTaskModels")
                .Key(workTaskModel["Id"])
                .Set(new { Code = "Test2" })
                .UpdateEntryAsync();

            Assert.Equal("Test2", workTaskModel["Code"]);
        }

        [Fact]
        public async Task UpdateWorkTaskModelWithEmptyLists()
        {
            var workTaskModel = await _client
                .For("WorkTaskModels")
                .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0f},{"Longitude", 2.0f}}  },
                })
                .InsertEntryAsync();

            workTaskModel = await _client
                .For("WorkTaskModels")
                .Key(workTaskModel["Id"])
                .Set(new Entry() { {"Code", "Test2"}, {"Attachments", new List<IDictionary<string, object>>()}, {"WorkActivityReports", null } })
                .UpdateEntryAsync();

            Assert.Equal("Test2", workTaskModel["Code"]);
        }

        [Fact]
        public async Task UpdateWorkTaskModelWholeObject()
        {
            var workTaskModel = await _client
                .For("WorkTaskModels")
                .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0f},{"Longitude", 2.0f}}  },
                })
                .InsertEntryAsync();

            workTaskModel["Code"] = "Test2";
            workTaskModel["Attachments"] = new List<IDictionary<string, object>>();
            workTaskModel["WorkActivityReports"] = null;
            workTaskModel = await _client
                .For("WorkTaskModels")
                .Key(workTaskModel["Id"])
                .Set(workTaskModel)
                .UpdateEntryAsync();

            Assert.Equal("Test2", workTaskModel["Code"]);

            workTaskModel["Code"] = "Test3";
            workTaskModel["Attachments"] = null;
            workTaskModel["WorkActivityReports"] = new List<IDictionary<string, object>>();
            workTaskModel = await _client
                .For("WorkTaskModels")
                .Key(workTaskModel["Id"])
                .Set(workTaskModel)
                .UpdateEntryAsync();

            Assert.Equal("Test3", workTaskModel["Code"]);
        }
    }
}
