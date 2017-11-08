using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Simple.OData.Client.TestUtils;
using Simple.OData.NorthwindModel;

namespace Simple.OData.Client.Tests
{
    public class TestBase : IDisposable
    {
        protected Uri _serviceUri;
        protected TestService _service;
        protected IODataClient _client;
        protected readonly bool _readOnlyTests;

        protected TestBase(bool readOnlyTests = false)
        {
            _readOnlyTests = readOnlyTests;
#if MOCK_HTTP_
            _serviceUri = new Uri("http://localhost/");
#else
            _service = new TestService(typeof(NorthwindService));
            _serviceUri = _service.ServiceUri;
#endif
            _client = CreateClientWithDefaultSettings();
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

        protected IODataClient CreateClientWithDefaultSettings()
        {
            return new ODataClient(new ODataClientSettings
            {
                BaseUri = _serviceUri,
                OnTrace = (x, y) => Console.WriteLine(string.Format(x, y)),
            });
        }

        protected IODataClient CreateClientWithCustomSettings(ODataClientSettings settings)
        {
            return new ODataClient(settings);
        }

        protected IODataClient CreateClientWithNameResolver(INameMatchResolver nameMatchResolver)
        {
                string metadataString =
#if MOCK_HTTP_
                    GetResourceAsString(@"Resources." + "Northwind.xml");
#else
                    null;
#endif
            return new ODataClient(new ODataClientSettings
            {
                BaseUri = _serviceUri,
                MetadataDocument = metadataString,
                OnTrace = (x, y) => Console.WriteLine(string.Format(x, y)),
                NameMatchResolver = nameMatchResolver,
            });
        }

        public void Dispose()
        {
#if !MOCK_HTTP_
            if (_client != null && !_readOnlyTests)
            {
                DeleteTestData().Wait();
        }
#endif

            if (_service != null)
            {
                _service.Dispose();
                _service = null;
            }
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

        public async Task<T> FindEntryAsync<T>(IBoundClient<T> command)
            where T : class
        {
#if MOCK_HTTP
            var request = await command
                .BuildRequestFor()
                .FindEntryAsync();
            using (var response = await request.RunAsync())
                return await response.ReadAsSingleAsync();
#else
            return await command.FindEntryAsync();
#endif
        }

        public async Task<IEnumerable<T>> FindEntriesAsync<T>(IBoundClient<T> command, ODataFeedAnnotations annotations = null)
            where T : class
        {
#if MOCK_HTTP
            var request = await command
                .BuildRequestFor()
                .FindEntriesAsync(annotations);
            using (var response = await request.RunAsync())
                return await response.ReadAsCollectionAsync(annotations);
#else
            return await command.FindEntriesAsync(annotations);
#endif
        }

        public async Task<U> FindScalarAsync<T,U>(IBoundClient<T> command)
            where T : class
        {
#if MOCK_HTTP
            var request = await command
                .BuildRequestFor()
                .FindEntriesAsync(true);
            using (var response = await request.RunAsync())
                return await response.ReadAsScalarAsync<U>();
#else
            return await command.FindScalarAsync<U>();
#endif
        }

        public async Task<T> InsertEntryAsync<T>(IBoundClient<T> command, bool resultRequired = true)
            where T : class
        {
#if MOCK_HTTP
            var request = await command
                .BuildRequestFor()
                .InsertEntryAsync();
            using (var response = await request.RunAsync())
                return await response.ReadAsSingleAsync();
#else
            return await command.InsertEntryAsync(true);
#endif
        }

        public async Task<T> UpdateEntryAsync<T>(IBoundClient<T> command, bool resultRequired = true)
            where T : class
        {
#if MOCK_HTTP
            var request = await command
                .BuildRequestFor()
                .UpdateEntryAsync();
            using (var response = await request.RunAsync())
                return await response.ReadAsSingleAsync();
#else
            return await command.UpdateEntryAsync(true);
#endif
        }
    }
}
