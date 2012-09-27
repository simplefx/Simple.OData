using System;
using System.Collections.Generic;
using System.Linq;
using Simple.Data.OData.NorthwindModel;
using Simple.OData.TestUtils;

namespace Simple.OData.Client.Tests
{
    public class TestBase : IDisposable
    {
        protected TestService _service;
        protected ODataClient _client;

        public TestBase()
        {
            _service = new TestService(typeof(NorthwindService));
            _client = new ODataClient(_service.ServiceUri.AbsoluteUri);
        }

        public void Dispose()
        {
            IEnumerable<dynamic> products = _client.FindEntries("Products");
            products.ToList().ForEach(x =>
                {
                    if (x["ProductName"].ToString().StartsWith("Test")) _client.DeleteEntry("Products", x);
                });
            IEnumerable<dynamic> categories = _client.FindEntries("Categories");
            categories.ToList().ForEach(x =>
            {
                if (x["CategoryName"].ToString().StartsWith("Test")) _client.DeleteEntry("Categories", x);
            });

            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
        }
    }
}
