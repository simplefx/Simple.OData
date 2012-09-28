using System;
using System.Collections.Generic;
using System.Linq;
# if !NETFX_CORE
using Simple.Data.OData.NorthwindModel;
using Simple.OData.TestUtils;
#endif

namespace Simple.OData.Client.Tests
{
    public class TestBase : IDisposable
    {
# if !NETFX_CORE
        protected TestService _service;
#endif
        protected ODataClient _client;

        public TestBase()
        {
#if !NETFX_CORE
            _service = new TestService(typeof(NorthwindService));
            _client = new ODataClient(_service.ServiceUri.AbsoluteUri);
#endif
        }

        public void Dispose()
        {
            if (_client != null)
            {
                IEnumerable<dynamic> products = _client.FindEntries("Products");
                foreach (var product in products)
                {
                    if (product["ProductName"].ToString().StartsWith("Test"))
                        _client.DeleteEntry("Products", product);
                }
                IEnumerable<dynamic> categories = _client.FindEntries("Categories");
                foreach (var category in categories)
                {
                    if (category["CategoryName"].ToString().StartsWith("Test"))
                        _client.DeleteEntry("Categories", category);
                }
            }

#if !NETFX_CORE
            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
#endif
        }
    }
}
