using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Simple.OData.Client.TestUtils;
using Simple.OData.NorthwindModel;

namespace Simple.OData.Client.Tests
{
    public class TestBase : IDisposable
    {
        private const string MockDataDir = @"..\..\MockData";
        protected Uri _serviceUri;
        protected TestService _service;
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
                MetadataDocument = GetMetadataDocument(),
                OnTrace = (x, y) => Console.WriteLine(string.Format(x, y)),
            });
        }

        protected IODataClient CreateClientWithCustomSettings(ODataClientSettings settings)
        {
            return new ODataClient(settings);
        }

        protected IODataClient CreateClientWithNameResolver(INameMatchResolver nameMatchResolver)
        {
            return new ODataClient(new ODataClientSettings
            {
                BaseUri = _serviceUri,
                MetadataDocument = GetMetadataDocument(),
                OnTrace = (x, y) => Console.WriteLine(string.Format(x, y)),
                NameMatchResolver = nameMatchResolver,
            });
        }

        public void Dispose()
        {
#if !MOCK_HTTP
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

        public async Task<IClientWithResponse<T>> MockRequestAsync<T>(IClientWithRequest<T> request)
            where T : class
        {
            var testMethodName = GetTestMethodFullName();
            var hasMockData = File.Exists(GetMockDataPath(testMethodName));
            if (hasMockData)
                await ValidateRequestAsync(testMethodName, request.GetRequest());
            else
                await SaveRequestAsync(testMethodName, request.GetRequest());
            if (hasMockData)
            {
                return request.FromResponse(await GetMockResponseAsync(testMethodName));
            }
            else
            {
                var response = await request.RunAsync();
                await SaveResponseAsync(testMethodName, await response.GetResponseStreamAsync());
                return response;
            }
        }

        public async Task<T> FindEntryAsync<T>(IBoundClient<T> command)
            where T : class
        {
#if MOCK_HTTP
            var request = await command
                .BuildRequestFor()
                .FindEntryAsync();
            var response = await MockRequestAsync(request);
            return await response.ReadAsSingleAsync();
#else
            return await command.FindEntryAsync();
#endif
        }

        public async Task<IEnumerable<T>> FindEntriesAsync<T>(IBoundClient<T> command,
            ODataFeedAnnotations annotations = null)
            where T : class
        {
#if MOCK_HTTP
            var request = await command
                .BuildRequestFor()
                .FindEntriesAsync(annotations);
            var response = await MockRequestAsync(request);
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
            var response = await MockRequestAsync(request);
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
            var response = await MockRequestAsync(request);
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
            var response = await MockRequestAsync(request);
            return await response.ReadAsSingleAsync();
#else
            return await command.UpdateEntryAsync(true);
#endif
        }

        private string GetMockDataPath(string testMethodName)
        {
            return Path.Combine(MockDataDir, testMethodName + ".txt");
        }

        private async Task SaveRequestAsync(string testMethodName, ODataRequest request)
        {
            using (var writer = new StreamWriter(GetMockDataPath(testMethodName), false))
            {
                await writer.WriteLineAsync($"--- Request ---");
                var requestMessage = request.RequestMessage;
                await writer.WriteLineAsync($"{requestMessage.Method} {requestMessage.RequestUri.AbsolutePath}");
                foreach (var header in requestMessage.Headers)
                    await writer.WriteLineAsync($"{header.Key}: {header.Value}");
                if (requestMessage.Content != null)
                {
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync(await requestMessage.Content.ReadAsStringAsync());
                }
            }
        }

        private async Task SaveResponseAsync(string testMethodName, Stream responseStream)
        {
            using (var writer = new StreamWriter(GetMockDataPath(testMethodName), true))
            {
                await writer.WriteLineAsync();
                await writer.WriteLineAsync($"--- Response ---");
                await writer.WriteLineAsync(Utils.StreamToString(responseStream));
            }
        }

        private async Task ValidateRequestAsync(string testMethodName, ODataRequest request)
        {
            using (var reader = new StreamReader(GetMockDataPath(testMethodName)))
            {
                var line = await reader.ReadLineAsync();
                while (line != "--- Request ---")
                {
                    if (line == null)
                        throw new Exception("Request mock data not found");
                    line = await reader.ReadLineAsync();
                }
                var requestMessage = request.RequestMessage;
                line = await reader.ReadLineAsync();
                var splitPos = line.IndexOf(' ');
                var expectedMethod = line.Substring(0, splitPos);
                Assert.Equal(expectedMethod, requestMessage.Method.ToString());
                var expectedUri = line.Substring(splitPos + 1);
                Assert.Equal(expectedUri, requestMessage.RequestUri.AbsolutePath);
            }
        }

        private async Task<HttpResponseMessage> GetMockResponseAsync(string testMethodName)
        {
            using (var reader = new StreamReader(GetMockDataPath(testMethodName)))
            {
                var line = await reader.ReadLineAsync();
                while (line != "--- Response ---")
                {
                    if (line == null)
                        throw new Exception("Response mock data not found");
                    line = await reader.ReadLineAsync();
                }
                var content = await reader.ReadToEndAsync();
                var responseMessage = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(content)
                };
                return responseMessage;
            }
        }

        private string GetTestMethodFullName()
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            var baseType = stackTrace.GetFrame(0).GetMethod().DeclaringType;
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
}
