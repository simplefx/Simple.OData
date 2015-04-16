using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

using Entry = System.Collections.Generic.Dictionary<string, object>;

namespace Simple.OData.Client.Tests
{
    public abstract class WebApiTestsBase : IDisposable
    {
        protected readonly IODataClient _client;

        protected WebApiTestsBase(ODataClientSettings settings)
        {
            _client = new ODataClient(settings);
        }

        public void Dispose()
        {
            if (_client != null)
            {
                DeleteTestData().Wait();
            }
        }

        private async Task DeleteTestData()
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

        [Fact]
        public async Task GetProductsCount()
        {
            await DeleteTestData();

            var products = await _client
                .For("Products")
                .FindEntriesAsync();

            Assert.Equal(5, products.Count());
        }

        [Fact]
        public async Task InsertProduct()
        {
            await DeleteTestData();

            var product = await _client
                .For("Products")
                .Set(new Entry() { { "Name", "Test1" }, { "Price", 18m } })
                .InsertEntryAsync();

            Assert.Equal("Test1", product["Name"]);
        }

        [Fact]
        public async Task UpdateProduct()
        {
            await DeleteTestData();

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
            await DeleteTestData();

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
            await DeleteTestData();

            var workTaskModel = await _client
                .For("WorkTaskModels")
                .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0d},{"Longitude", 2.0d}}  },
                })
                .InsertEntryAsync();

            Assert.Equal("Test1", workTaskModel["Code"]);
        }

        [Fact]
        public async Task UpdateWorkTaskModel()
        {
            await DeleteTestData();

            var workTaskModel = await _client
                .For("WorkTaskModels")
                .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0d},{"Longitude", 2.0d}}  },
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
            await DeleteTestData();

            var workTaskModel = await _client
                .For("WorkTaskModels")
                .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0d},{"Longitude", 2.0d}}  },
                })
                .InsertEntryAsync();

            workTaskModel = await _client
                .For("WorkTaskModels")
                .Key(workTaskModel["Id"])
                .Set(new Entry() { { "Code", "Test2" }, { "Attachments", new List<IDictionary<string, object>>() }, { "WorkActivityReports", null } })
                .UpdateEntryAsync();

            Assert.Equal("Test2", workTaskModel["Code"]);
        }

        [Fact]
        public async Task UpdateWorkTaskModelWholeObject()
        {
            await DeleteTestData();

            var workTaskModel = await _client
                .For("WorkTaskModels")
                .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0d},{"Longitude", 2.0d}}  },
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

#if ODATA_V3
    public class WebApiTests : WebApiTestsBase
    {
        public WebApiTests()
            : base(new ODataClientSettings(new Uri("http://va-odata-integration.azurewebsites.net/odata/open")))
        {
        }

    }

    public class WebApiWithAuthenticationTests : WebApiTestsBase
    {
        private const string _user = "tester";
        private const string _password = "tester123";

        public WebApiWithAuthenticationTests()
            : base(new ODataClientSettings()
            {
                BaseUri = new Uri("http://va-odata-integration.azurewebsites.net/odata/secure"), 
                Credentials = new NetworkCredential(_user, _password)
            })
        {
        }
    }
#endif
}
