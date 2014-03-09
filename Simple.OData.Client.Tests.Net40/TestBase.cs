using System;
using System.Collections.Generic;
using System.Linq;
# if !NETFX_CORE
using Simple.OData.Client.TestUtils;
using Simple.OData.NorthwindModel;
#endif

namespace Simple.OData.Client.Tests
{
    public class TestBase : IDisposable
    {
        protected string _serviceUri;
# if !NETFX_CORE
        protected TestService _service;
#endif
        protected IODataClient _client;

        public TestBase()
        {
#if NETFX_CORE
            _serviceUri = "http://NORTHWIND/Northwind.svc/";
#else
            _service = new TestService(typeof(NorthwindService));
            _serviceUri = _service.ServiceUri.AbsoluteUri;
#endif
            _client = CreateClientWithDefaultSettings();
        }

        public IODataClient CreateClientWithDefaultSettings()
        {
            return new ODataClient(_serviceUri);
        }

        public void Dispose()
        {
            if (_client != null)
            {
                var products = _client.FindEntries("Products");
                foreach (var product in products)
                {
                    if (product["ProductName"].ToString().StartsWith("Test"))
                        _client.DeleteEntry("Products", product);
                }
                var categories = _client.FindEntries("Categories");
                foreach (var category in categories)
                {
                    if (category["CategoryName"].ToString().StartsWith("Test"))
                        _client.DeleteEntry("Categories", category);
                }
                var transports = _client.FindEntries("Transport");
                foreach (var transport in transports)
                {
                    if (int.Parse(transport["TransportID"].ToString()) > 2)
                        _client.DeleteEntry("Transport", transport);
                }
                var employees = _client.FindEntries("Employees");
                foreach (var employee in employees)
                {
                    if (employee["LastName"].ToString().StartsWith("Test"))
                        _client.DeleteEntry("Employees", employee);
                }
            }

#if NETFX_CORE
#else
            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
#endif
        }
    }
}
