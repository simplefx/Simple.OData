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
#endif
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
