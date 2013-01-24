using System;
using System.Linq;
using System.Net;
using Xunit;

namespace Simple.OData.Client.Tests
{
    using Entry = System.Collections.Generic.Dictionary<string, object>;

    public class ODataClientTests : TestBase
    {
        [Fact]
        public void FindEntries()
        {
            var products = _client.FindEntries("Products");
            Assert.True(products.Count() > 0);
        }

        [Fact]
        public void FindEntryExisting()
        {
            var product = _client.FindEntry("Products?$filter=ProductName eq 'Chai'");
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void FindEntryExistingExpressionFilter()
        {
            var x = ODataFilter.Expression;
            string filter = _client.FormatFilter("Products", x.ProductName == "Chai");
            var product = _client.FindEntry(filter);
            Assert.Equal("Chai", product["ProductName"]);
        }

        [Fact]
        public void FindEntryNonExisting()
        {
            var product = _client.FindEntry("Products?$filter=ProductName eq 'XYZ'");
            Assert.Null(product);
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
        public void InsertEntryWithResult()
        {
            var product = _client.InsertEntry("Products", new Entry() {{"ProductName", "Test1"}, {"UnitPrice", 18m}}, true);

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
            var ship = _client.InsertEntry("Transport", new Entry() { { "ShipName", "Test1" } }, true, "Ships");

            Assert.Equal("Test1", ship["ShipName"]);
        }

        [Fact]
        public void UpdateEntry()
        {
            var key = new Entry() {{"ProductID", 1}};
            _client.UpdateEntry("Products", key, new Entry() { { "ProductName", "Chai" }, { "UnitPrice", 123m } });

            var product = _client.GetEntry("Products", key);
            Assert.Equal(123m, product["UnitPrice"]);
        }

        [Fact]
        public void UpdateEntrySubcollection()
        {
            var key = new Entry() { { "TransportID", 1 } };
            _client.UpdateEntry("Transport", key, new Entry() { { "ShipName", "Test2" } }, "Ships");

            var ship = _client.GetEntry("Transport", key);
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
            var ship = _client.InsertEntry("Transport", new Entry() { { "ShipName", "Test3" } }, true, "Ships");
            ship = _client.FindEntry("Transport?$filter=TransportID eq " + ship["TransportID"]);
            Assert.NotNull(ship);

            _client.DeleteEntry("Transport", ship);

            ship = _client.FindEntry("Transport?$filter=TransportID eq " + ship["TransportID"]);
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
        public void InterceptRequest()
        {
            _client.RequestInterceptor = x => x.Method = "PUT";
            Assert.Throws<WebRequestException>(() => _client.FindEntries("Products"));
            _client.RequestInterceptor = null;
        }

        [Fact]
        public void InterceptResponse()
        {
            _client.ResponseInterceptor = x => { throw new InvalidOperationException(); };
            Assert.Throws<InvalidOperationException>(() => _client.FindEntries("Products"));
            _client.ResponseInterceptor = null;
        }
    }
}