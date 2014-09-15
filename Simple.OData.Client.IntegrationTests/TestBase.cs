using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.Tests
{
    public abstract class TestBase : IDisposable
    {
        protected const string ODataV2ReadWriteUri = "http://services.odata.org/V2/%28S%28vudzeuasumowublegj2gmjsw%29%29/OData/OData.svc/";
        protected const string ODataV3ReadOnlyUri = "http://services.odata.org/V3/OData/OData.svc/";
        protected const string ODataV3ReadWriteUri = "http://services.odata.org/V3/%28S%28syfftau5khfkto4clyuy5mab%29%29/OData/OData.svc/";
        protected const string ODataV4ReadOnlyUri = "http://services.odata.org/V4/OData/OData.svc/";
        protected const string ODataV4ReadWriteUri = "http://services.odata.org/V4/OData/%28S%28kupktsrj4v01rxzddeyf1bwh%29%29/OData.svc/";

        protected const string NorthwindV2ReadOnlyUri = "http://services.odata.org/V2/Northwind/Northwind.svc/";
        protected const string NorthwindV3ReadOnlyUri = "http://services.odata.org/V3/Northwind/Northwind.svc/";
        protected const string NorthwindV4ReadOnlyUri = "http://services.odata.org/V4/Northwind/Northwind.svc/";

        protected readonly string _serviceUri;
        protected readonly ODataPayloadFormat _payloadFormat;
        protected IODataClient _client;

        protected TestBase(string serviceUri, ODataPayloadFormat payloadFormat)
        {
            _serviceUri = serviceUri;
            _payloadFormat = payloadFormat;
            _client = CreateClientWithDefaultSettings();
        }

        protected IODataClient CreateClientWithDefaultSettings()
        {
            return new ODataClient(new ODataClientSettings(_serviceUri) {PayloadFormat = _payloadFormat});
        }

        public void Dispose()
        {
            if (_client != null)
            {
                DeleteTestData().Wait();
            }
        }

        protected abstract Task DeleteTestData();

        public async static Task AssertThrowsAsync<T>(Func<Task> testCode) where T : Exception
        {
            try
            {
                await testCode();
                throw new Exception(string.Format("Expected exception: {0}", typeof (T)));
            }
            catch (T)
            {
            }
            catch (AggregateException exception)
            {
                Assert.IsType<T>(exception.InnerExceptions.Single());
            }
        }
    }
}
