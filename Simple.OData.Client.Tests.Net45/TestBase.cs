using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Simple.OData.Client.TestUtils;
using Simple.OData.NorthwindModel;

namespace Simple.OData.Client.Tests
{
    public static class ODataClientSettingsExtensionMethods
    {
        private const string MockDataDir = @"..\..\..\MockData";

        public static ODataClientSettings WithNameResolver(this ODataClientSettings settings, INameMatchResolver resolver)
        {
            settings.NameMatchResolver = resolver;
            return settings;
        }

        public static ODataClientSettings WithAnnotations(this ODataClientSettings settings)
        {
            settings.IncludeAnnotationsInResults = true;
            return settings;
        }

        public static ODataClientSettings WithIgnoredUnmappedProperties(this ODataClientSettings settings)
        {
            settings.IgnoreUnmappedProperties = true;
            return settings;
        }

        public static ODataClientSettings WithIgnoredResourceNotFoundException(this ODataClientSettings settings)
        {
            settings.IgnoreResourceNotFoundException = true;
            return settings;
        }

        public static ODataClientSettings WithRequestInterceptor(this ODataClientSettings settings, Action<HttpRequestMessage> action)
        {
            settings.BeforeRequest = action;
            return settings;
        }

        public static ODataClientSettings WithResponseInterceptor(this ODataClientSettings settings, Action<HttpResponseMessage> action)
        {
            settings.AfterResponse = action;
            return settings;
        }

        public static ODataClientSettings WithHttpMock(this ODataClientSettings settings)
        {
            var methodName = GetTestMethodFullName();
            var mockDataPath = GetMockDataPath(methodName);
            var recording = !File.Exists(mockDataPath);
            var requestExecutor = new MockingRequestExecutor(settings, mockDataPath, recording);
            settings.RequestExecutor = requestExecutor.ExecuteRequestAsync;
            return settings;
        }

        private static string GetMockDataPath(string testMethodName)
        {
            return Path.Combine(MockDataDir, testMethodName + ".txt");
        }

        private static string GetTestMethodFullName()
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            var baseType = typeof(TestBase);
            for (var frameNumber = 1; ; frameNumber++)
            {
                var stackFrame = stackTrace.GetFrame(frameNumber);
                if (stackFrame == null)
                    throw new InvalidOperationException("Attempt to retrieve a frame beyond the call stack");
                var method = stackFrame.GetMethod();
                var methodName = new string(method.Name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
                if (method.DeclaringType != baseType && baseType.IsAssignableFrom(method.DeclaringType))
                    return string.Format($"{method.DeclaringType.Name}.{methodName}");
            }
        }
    }

    public class TestBase : IDisposable
    {
        protected Uri _serviceUri;
#if !MOCK_HTTP
        protected TestService _service;
#endif
        protected IODataClient _client;
        protected readonly bool _readOnlyTests;

        protected TestBase(bool readOnlyTests = false)
        {
            _readOnlyTests = readOnlyTests;
#if MOCK_HTTP
            _serviceUri = new Uri("http://localhost/");
#else
            _service = new TestService(typeof(NorthwindService));
            _serviceUri = _service.ServiceUri;
#endif
            _client = new ODataClient(CreateDefaultSettings());
        }

        /* Original Northwind database
         * protected const int ExpectedCountOfProducts = 77;
         * protected const int ExpectedCountOfBeveragesProducts = 12;
         * protected const int ExpectedCountOfCondimentsProducts = 12;
         * protected const int ExpectedCountOfOrdersHavingAnyDetail = 160;
         * protected const int ExpectedCountOfOrdersHavingAllDetails = 11;
         * protected const int ExpectedCountOfProductsWithOrdersHavingAnyDetail = 160;
         * protected const int ExpectedCountOfProductsWithOrdersHavingAllDetails = 11;
        */

        protected const int ExpectedCountOfProducts = 22;
        protected const int ExpectedCountOfBeveragesProducts = 2;
        protected const int ExpectedCountOfCondimentsProducts = 7;
        protected const int ExpectedCountOfOrdersHavingAnyDetail = 5;
        protected const int ExpectedCountOfOrdersHavingAllDetails = 6;
        protected const int ExpectedCountOfProductsWithOrdersHavingAnyDetail = 5;
        protected const int ExpectedCountOfProductsWithOrdersHavingAllDetails = 6;

        protected ODataClientSettings CreateDefaultSettings()
        {
            return new ODataClientSettings
            {
                BaseUri = _serviceUri,
                MetadataDocument = GetMetadataDocument(),
                OnTrace = (x, y) => Console.WriteLine(string.Format(x, y)),
            };
        }

        public void Dispose()
        {
#if !MOCK_HTTP
            if (_client != null && !_readOnlyTests)
            {
                DeleteTestData().Wait();
            }

            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
#endif
        }

        private static string GetResourceAsString(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            var completeResourceName = resourceNames.FirstOrDefault(o => o.EndsWith("." + resourceName, StringComparison.CurrentCultureIgnoreCase));
            using (var resourceStream = assembly.GetManifestResourceStream(completeResourceName))
            {
                var reader = new StreamReader(resourceStream);
                return reader.ReadToEnd();
            }
        }

        private string GetMetadataDocument()
        {
#if MOCK_HTTP
            return GetResourceAsString(@"Resources." + "Northwind.xml");
#else
            return null;
#endif

        }

        private async Task DeleteTestData()
        {
            var products = await _client.FindEntriesAsync("Products");
            foreach (var product in products)
            {
                var productName = product["ProductName"] as string;
                if (string.IsNullOrEmpty(productName) || productName.StartsWith("Test"))
                    await _client.DeleteEntryAsync("Products", product);
            }
            var categories = await _client.FindEntriesAsync("Categories");
            foreach (var category in categories)
            {
                var categoryName = category["CategoryName"] as string;
                if (string.IsNullOrEmpty(categoryName) || categoryName.StartsWith("Test"))
                    await _client.DeleteEntryAsync("Categories", category);
            }
            var transports = await _client.FindEntriesAsync("Transport");
            foreach (var transport in transports)
            {
                if (int.Parse(transport["TransportID"].ToString()) > 2)
                    await _client.DeleteEntryAsync("Transport", transport);
            }
            var employees = await _client.FindEntriesAsync("Employees");
            foreach (var employee in employees)
            {
                var employeeName = employee["LastName"] as string;
                if (string.IsNullOrEmpty(employeeName) || employeeName.StartsWith("Test"))
                    await _client.DeleteEntryAsync("Employees", employee);
            }
        }

        public static async Task AssertThrowsAsync<T>(Func<Task> testCode) where T : Exception
        {
            try
            {
                await testCode();
                throw new Exception($"Expected exception: {typeof(T)}");
            }
            catch (T)
            {
            }
            catch (AggregateException exception)
            {
                var innerException = exception.InnerExceptions.Single();
                Assert.IsType<T>(innerException);
            }
        }
    }
}
