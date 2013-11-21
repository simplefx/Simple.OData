using System;
using System.Linq;
using Xunit;

namespace Simple.OData.Client.Tests
{
    using Entry = System.Collections.Generic.Dictionary<string, object>;

    public class ClientTests : TestBase
    {
        [Fact]
        public void FindEntries()
        {
            var products = _client.FindEntries("Products");
            Assert.True(products.Count() > 0);
        }

        [Fact]
        public void FindEntriesNonExisting()
        {
            var products = _client.FindEntries("Products?$filter=ProductID eq -1");
            Assert.True(products.Count() == 0);
        }

        [Fact]
        public void FindEntriesNonExistingLong()
        {
            var products = _client.FindEntries("Products?$filter=ProductID eq 999999999999L");
            Assert.True(products.Count() == 0);
        }

        [Fact]
        public void FindEntryExisting()
        {
            var product = _client.FindEntry("Products?$filter=ProductName eq 'Chai'");
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void FindEntryNonExisting()
        {
            var product = _client.FindEntry("Products?$filter=ProductName eq 'XYZ'");
            Assert.Null(product);
        }

        [Fact]
        public void FindEntryNuGetV1()
        {
            var client = new ODataClient("http://nuget.org/api/v1");
            var package = client.FindEntry("Packages?$filter=Title eq 'EntityFramework'");
            Assert.NotNull(package["Id"]);
        }

        [Fact]
        public void FindEntryNuGetV2()
        {
            var client = new ODataClient("http://nuget.org/api/v2");
            var package = client.FindEntry("Packages?$filter=Title eq 'EntityFramework'");
            Assert.NotNull(package["Id"]);
        }

        [Fact]
        public void GetEntryExisting()
        {
            var product = _client.GetEntry("Products", new Entry() { { "ProductID", 1 } });
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void GetEntryExistingCompoundKey()
        {
            var orderDetail = _client.GetEntry("OrderDetails", new Entry() { { "OrderID", 10248 }, { "ProductID", 11 } });
            Assert.Equal(11, orderDetail["ProductID"]);
        }

        [Fact]
        public void GetEntryNonExisting()
        {
            Assert.Throws<WebRequestException>(() => _client.GetEntry("Products", new Entry() { { "ProductID", -1 } }));
        }

        [Fact]
        public void GetEntryNonExistingIgnoreException()
        {
            var settings = new ODataClientSettings
            {
                UrlBase = _serviceUri,
                IgnoreResourceNotFoundException = true,
            };
            var client = new ODataClient(settings);
            var product = client.GetEntry("Products", new Entry() {{"ProductID", -1}});

            Assert.Null(product);
        }

        [Fact]
        public void InsertEntryWithResult()
        {
            var product = _client.InsertEntry("Products", new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 18m } }, true);

            Assert.Equal("Test1", product["ProductName"]);
        }

        [Fact]
        public void InsertEntryNoResult()
        {
            var product = _client.InsertEntry("Products", new Entry() { { "ProductName", "Test2" }, { "UnitPrice", 18m } }, false);

            Assert.Null(product);
        }

        [Fact]
        public void InsertEntrySubcollection()
        {
            var ship = _client.InsertEntry("Transport/Ships", new Entry() { { "ShipName", "Test1" } }, true);

            Assert.Equal("Test1", ship["ShipName"]);
        }

        [Fact]
        public void UpdateEntry()
        {
            var key = new Entry() { { "ProductID", 1 } };
            _client.UpdateEntry("Products", key, new Entry() { { "ProductName", "Chai" }, { "UnitPrice", 123m } });

            var product = _client.GetEntry("Products", key);
            Assert.Equal(123m, product["UnitPrice"]);
        }

        [Fact]
        public void UpdateEntrySubcollection()
        {
            var ship = _client.InsertEntry("Transport/Ships", new Entry() { { "ShipName", "Test1" } }, true);
            var key = new Entry() { { "TransportID", ship["TransportID"] } };
            _client.UpdateEntry("Transport/Ships", key, new Entry() { { "ShipName", "Test2" } });

            ship = _client.GetEntry("Transport", key);
            Assert.Equal("Test2", ship["ShipName"]);
        }

        [Fact]
        public void UpdateEntrySubcollectionWithResourceType()
        {
            var clientSettings = new ODataClientSettings
            {
                UrlBase = _serviceUri,
                IncludeResourceTypeInEntryProperties = true,
            };
            var client = new ODataClient(clientSettings);
            var ship = client.InsertEntry("Transport/Ships", new Entry() { { "ShipName", "Test1" } }, true);
            var key = new Entry() { { "TransportID", ship["TransportID"] } };
            client.UpdateEntry("Transport/Ships", key, new Entry() { { "ShipName", "Test2" }, { ODataCommand.ResourceTypeLiteral, "Ships" } });

            ship = client.GetEntry("Transport", key);
            Assert.Equal("Test2", ship["ShipName"]);
        }

        [Fact]
        public void DeleteEntry()
        {
            var product = _client.InsertEntry("Products", new Entry() { { "ProductName", "Test3" }, { "UnitPrice", 18m } }, true);
            product = _client.FindEntry("Products?$filter=ProductName eq 'Test3'");
            Assert.NotNull(product);

            _client.DeleteEntry("Products", product);

            product = _client.FindEntry("Products?$filter=ProductName eq 'Test3'");
            Assert.Null(product);
        }

        [Fact]
        public void DeleteEntrySubCollection()
        {
            var ship = _client.InsertEntry("Transport/Ships", new Entry() { { "ShipName", "Test3" } }, true);
            ship = _client.FindEntry("Transport?$filter=TransportID eq " + ship["TransportID"]);
            Assert.NotNull(ship);

            _client.DeleteEntry("Transport", ship);

            ship = _client.FindEntry("Transport?$filter=TransportID eq " + ship["TransportID"]);
            Assert.Null(ship);
        }

        [Fact]
        public void DeleteEntrySubCollectionWithResourceType()
        {
            var clientSettings = new ODataClientSettings
            {
                UrlBase = _serviceUri,
                IncludeResourceTypeInEntryProperties = true,
            };
            var client = new ODataClient(clientSettings);
            var ship = client.InsertEntry("Transport/Ships", new Entry() { { "ShipName", "Test3" } }, true);
            ship = client.FindEntry("Transport?$filter=TransportID eq " + ship["TransportID"]);
            Assert.NotNull(ship);

            client.DeleteEntry("Transport", ship);

            ship = client.FindEntry("Transport?$filter=TransportID eq " + ship["TransportID"]);
            Assert.Null(ship);
        }

        [Fact]
        public void LinkEntry()
        {
            var category = _client.InsertEntry("Categories", new Entry() { { "CategoryName", "Test4" } }, true);
            var product = _client.InsertEntry("Products", new Entry() { { "ProductName", "Test5" } }, true);

            _client.LinkEntry("Products", product, "Category", category);

            product = _client.FindEntry("Products?$filter=ProductName eq 'Test5'");
            Assert.NotNull(product["CategoryID"]);
            Assert.Equal(category["CategoryID"], product["CategoryID"]);
        }

        [Fact]
        public void UnlinkEntry()
        {
            var category = _client.InsertEntry("Categories", new Entry() { { "CategoryName", "Test6" } }, true);
            var product = _client.InsertEntry("Products", new Entry() { { "ProductName", "Test7" }, { "CategoryID", category["CategoryID"] } }, true);
            product = _client.FindEntry("Products?$filter=ProductName eq 'Test7'");
            Assert.NotNull(product["CategoryID"]);
            Assert.Equal(category["CategoryID"], product["CategoryID"]);

            _client.UnlinkEntry("Products", product, "Category");

            product = _client.FindEntry("Products?$filter=ProductName eq 'Test7'");
            Assert.Null(product["CategoryID"]);
        }

        [Fact]
        public void ExecuteScalarFunctionWithStringParameter()
        {
            var result = _client.ExecuteFunctionAsScalar<int>("ParseInt", new Entry() { { "number", "1" } });
            Assert.Equal(1, result);
        }

        [Fact]
        public void ExecuteScalarFunctionWithLongParameter()
        {
            var result = _client.ExecuteFunctionAsScalar<long>("PassThroughLong", new Entry() { { "number", 1L } });
            Assert.Equal(1L, result);
        }

        [Fact]
        public void ExecuteScalarFunctionWithDateTimeParameter()
        {
            var dateTime = new DateTime(2013, 1, 1, 12, 13, 14);
            var result = _client.ExecuteFunctionAsScalar<DateTime>("PassThroughDateTime", new Entry() { { "dateTime", dateTime } });
            Assert.Equal(dateTime.ToLocalTime(), result);
        }

        [Fact]
        public void ExecuteScalarFunctionWithGuidParameter()
        {
            var guid = Guid.NewGuid();
            var result = _client.ExecuteFunctionAsScalar<Guid>("PassThroughGuid", new Entry() { { "guid", guid } });
            Assert.Equal(guid, result);
        }

        [Fact]
        public void BatchWithSuccess()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                client.InsertEntry("Products", new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 10m } }, false);
                client.InsertEntry("Products", new Entry() { { "ProductName", "Test2" }, { "UnitPrice", 20m } }, false);
                batch.Complete();
            }

            var product = _client.FindEntry("Products?$filter=ProductName eq 'Test1'");
            Assert.NotNull(product);
            product = _client.FindEntry("Products?$filter=ProductName eq 'Test2'");
            Assert.NotNull(product);
        }

        [Fact]
        public void BatchWithPartialFailures()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                client.InsertEntry("Products", new Entry() { { "ProductName", "Test1" }, { "UnitPrice", 10m } }, false);
                client.InsertEntry("Products", new Entry() { { "ProductName", "Test2" }, { "UnitPrice", 10m }, { "SupplierID", 0xFFFF } }, false);
                Assert.Throws<WebRequestException>(() => batch.Complete());
            }
        }

        [Fact]
        public void BatchWithAllFailures()
        {
            using (var batch = new ODataBatch(_serviceUri))
            {
                var client = new ODataClient(batch);
                client.InsertEntry("Products", new Entry() { { "UnitPrice", 10m } }, false);
                client.InsertEntry("Products", new Entry() { { "UnitPrice", 20m } }, false);
                Assert.Throws<WebRequestException>(() => batch.Complete());
            }
        }

        [Fact]
        public void InterceptRequest()
        {
            var settings = new ODataClientSettings
                               {
                                   UrlBase = _serviceUri,
                                   BeforeRequest = x => x.Method = "PUT",
                               };
            var client = new ODataClient(settings);
            Assert.Throws<WebRequestException>(() => client.FindEntries("Products"));
        }

        [Fact]
        public void InterceptResponse()
        {
            var settings = new ODataClientSettings
            {
                UrlBase = _serviceUri,
                AfterResponse = x => { throw new InvalidOperationException(); },
            };
            var client = new ODataClient(settings);
            Assert.Throws<InvalidOperationException>(() => client.FindEntries("Products"));
        }

        [Fact]
        public void FindEntryExistingDynamicFilter()
        {
            var x = ODataDynamic.Expression;
            string filter = _client.FormatCommand("Products", x.ProductName == "Chai");
            var product = _client.FindEntry(filter);
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void FindBaseClassEntryDynamicFilter()
        {
            var x = ODataDynamic.Expression;
            string filter = _client.FormatCommand("Transport", x.TransportID == 1);
            var ship = _client.FindEntry(filter);
            Assert.Equal("Titanic", ship["ShipName"]);
        }

        [Fact]
        public void FindDerivedClassEntryDynamicFilter()
        {
            var x = ODataDynamic.Expression;
            string filter = _client.FormatCommand("Transport/Ships", x.ShipName == "Titanic");
            var ship = _client.FindEntry(filter);
            Assert.Equal("Titanic", ship["ShipName"]);
        }

        [Fact]
        public void FindEntryExistingTypedFilter()
        {
            string filter = _client.FormatCommand<Product>("Products", x => x.ProductName == "Chai");
            var product = _client.FindEntry(filter);
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void FindBaseClassEntryTypedFilter()
        {
            string filter = _client.FormatCommand<Transport>("Transport", x => x.TransportID == 1);
            var ship = _client.FindEntry(filter);
            Assert.Equal("Titanic", ship["ShipName"]);
        }

        [Fact]
        public void FindDerivedClassEntryTypedFilter()
        {
            string filter = _client.FormatCommand<Ship>("Transport/Ships", x => x.ShipName == "Titanic");
            var ship = _client.FindEntry(filter);
            Assert.Equal("Titanic", ship["ShipName"]);
        }
    }
}
