using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public void Dispose()
        {
            if (_client != null)
            {
                var products = _client.FindEntries("Products");
                foreach (var product in products)
                {
                    if (product["Name"].ToString().StartsWith("Test"))
                        _client.DeleteEntry("Products", product);
                }

                var workTaskModels = _client.FindEntries("WorkTaskModels");
                foreach (var workTaskModel in workTaskModels)
                {
                    if (workTaskModel["Code"].ToString().StartsWith("Test"))
                        _client.DeleteEntry("workTaskModels", workTaskModel);
                }
            }
        }

        [Fact]
        public void GetProductsCount()
        {
            var products = _client
                .For("Products")
                .FindEntries();

            Assert.Equal(5, products.Count());
        }

        [Fact]
        public void InsertProduct()
        {
            var product = _client
                .For("Products")
                .Set(new Entry() { { "Name", "Test1" }, { "Price", 18m } })
                .InsertEntry();

            Assert.Equal("Test1", product["Name"]);
        }

        [Fact]
        public void UpdateProduct()
        {
            var product = _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntry();

            product = _client
                .For("Products")
                .Key(product["ID"])
                .Set(new { Price = 123m })
                .UpdateEntry();

            Assert.Equal(123m, product["Price"]);
        }

        [Fact]
        public void DeleteProduct()
        {
            var product = _client
                .For("Products")
                .Set(new { Name = "Test1", Price = 18m })
                .InsertEntry();

            _client
                .For("Products")
                .Key(product["ID"])
                .DeleteEntry();

            product = _client
                .For("Products")
                .Filter("Name eq 'Test1'")
                .FindEntry();

            Assert.Null(product);
        }

        [Fact]
        public void InsertWorkTaskModel()
        {
            var workTaskModel = _client
                .For("WorkTaskModels")
                .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0f},{"Longitude", 2.0f}}  },
                })
                .InsertEntry();

            Assert.Equal("Test1", workTaskModel["Code"]);
        }

        [Fact]
        public void UpdateWorkTaskModel()
        {
            var workTaskModel = _client
                .For("WorkTaskModels")
                .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0f},{"Longitude", 2.0f}}  },
                })
                .InsertEntry();

            workTaskModel = _client
                .For("WorkTaskModels")
                .Key(workTaskModel["Id"])
                .Set(new { Code = "Test2" })
                .UpdateEntry();

            Assert.Equal("Test2", workTaskModel["Code"]);
        }

        [Fact]
        public void UpdateWorkTaskModelWithEmptyLists()
        {
            var workTaskModel = _client
                .For("WorkTaskModels")
                .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0f},{"Longitude", 2.0f}}  },
                })
                .InsertEntry();

            workTaskModel = _client
                .For("WorkTaskModels")
                .Key(workTaskModel["Id"])
                .Set(new Entry() { {"Code", "Test2"}, {"Attachments", new List<IDictionary<string, object>>()}, {"WorkActivityReports", null } })
                .UpdateEntry();

            Assert.Equal("Test2", workTaskModel["Code"]);
        }

        [Fact]
        public void UpdateWorkTaskModelWholeObject()
        {
            var workTaskModel = _client
                .For("WorkTaskModels")
                .Set(new Entry()
                {
                    { "Id", Guid.NewGuid() }, 
                    { "Code", "Test1" }, 
                    { "StartDate", DateTime.Now.AddDays(-1) },
                    { "EndDate", DateTime.Now.AddDays(1) },
                    { "Location", new Entry() {{"Latitude", 1.0f},{"Longitude", 2.0f}}  },
                })
                .InsertEntry();

            workTaskModel["Code"] = "Test2";
            workTaskModel["Attachments"] = new List<IDictionary<string, object>>();
            workTaskModel["WorkActivityReports"] = null;
            workTaskModel = _client
                .For("WorkTaskModels")
                .Key(workTaskModel["Id"])
                .Set(workTaskModel)
                .UpdateEntry();

            Assert.Equal("Test2", workTaskModel["Code"]);

            workTaskModel["Code"] = "Test3";
            workTaskModel["Attachments"] = null;
            workTaskModel["WorkActivityReports"] = new List<IDictionary<string, object>>();
            workTaskModel = _client
                .For("WorkTaskModels")
                .Key(workTaskModel["Id"])
                .Set(workTaskModel)
                .UpdateEntry();

            Assert.Equal("Test3", workTaskModel["Code"]);
        }
    }
}
