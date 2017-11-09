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

        private string GetMetadataDocument()
        {
#if MOCK_HTTP_
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

        public async Task<IClientWithResponse<T>> MockRequestAsync<T>(string testMethodName, IClientWithRequest<T> request)
            where T : class
        {
            var hasMockData = File.Exists(GetMockDataPath(testMethodName));
            if (hasMockData)
                await ValidateRequestAsync(testMethodName, request.GetRequest());
            else
                await SaveRequestAsync(testMethodName, request.GetRequest());
            if (hasMockData)
            {
                return request.FromResponse(await GetMockResponseAsync(
                    testMethodName, request.GetRequest().RequestMessage));
            }
            else
            {
                var response = await request.RunAsync();
                await SaveResponseAsync(testMethodName, response.ResponseMessage);
                return response;
            }
        }

        public async Task<T> FindEntryAsync<T>(IBoundClient<T> command)
            where T : class
        {
#if MOCK_HTTP
            var testMethodName = GetTestMethodFullName();
            var request = await command
                .BuildRequestFor()
                .FindEntryAsync();
            var response = await MockRequestAsync(testMethodName, request);
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
            var testMethodName = GetTestMethodFullName();
            var request = await command
                .BuildRequestFor()
                .FindEntriesAsync(annotations);
            var response = await MockRequestAsync(testMethodName, request);
            return await response.ReadAsCollectionAsync(annotations);
#else
            return await command.FindEntriesAsync(annotations);
#endif
        }

        public async Task<U> FindScalarAsync<T,U>(IBoundClient<T> command)
            where T : class
        {
#if MOCK_HTTP
            var testMethodName = GetTestMethodFullName();
            var request = await command
                .BuildRequestFor()
                .FindEntriesAsync(true);
            var response = await MockRequestAsync(testMethodName, request);
            return await response.ReadAsScalarAsync<U>();
#else
            return await command.FindScalarAsync<U>();
#endif
        }

        public async Task<T> InsertEntryAsync<T>(IBoundClient<T> command, bool resultRequired = true)
            where T : class
        {
#if MOCK_HTTP
            var testMethodName = GetTestMethodFullName();
            var request = await command
                .BuildRequestFor()
                .InsertEntryAsync();
            var response = await MockRequestAsync(testMethodName, request);
            return await response.ReadAsSingleAsync();
#else
            return await command.InsertEntryAsync(true);
#endif
        }

        public async Task<T> UpdateEntryAsync<T>(IBoundClient<T> command, bool resultRequired = true)
            where T : class
        {
#if MOCK_HTTP
            var testMethodName = GetTestMethodFullName();
            var request = await command
                .BuildRequestFor()
                .UpdateEntryAsync();
            var response = await MockRequestAsync(testMethodName, request);
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
                await writer.WriteLineAsync();
                var requestMessage = request.RequestMessage;
                var methodName = requestMessage.Method.ToString();
                var commandText = requestMessage.RequestUri.AbsolutePath.Split('/').Last();
                await writer.WriteLineAsync($"Command: {methodName} {commandText}");
                await writer.WriteLineAsync();
                await writer.WriteLineAsync("Headers:");
                await WriteHeadersAsync(writer, requestMessage.Headers);
                await writer.WriteLineAsync();
                if (requestMessage.Content != null)
                {
                    await writer.WriteLineAsync("Content headers:");
                    await WriteHeadersAsync(writer, requestMessage.Content.Headers);
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync("Content:");
                    await writer.WriteLineAsync(await requestMessage.Content.ReadAsStringAsync());
                }
            }
        }

        private async Task SaveResponseAsync(string testMethodName, HttpResponseMessage responseMessage)
        {
            using (var writer = new StreamWriter(GetMockDataPath(testMethodName), true))
            {
                await writer.WriteLineAsync();
                await writer.WriteLineAsync($"--- Response ---");
                await writer.WriteLineAsync();
                await writer.WriteLineAsync($"Status: {responseMessage.StatusCode}");
                await writer.WriteLineAsync();
                await writer.WriteLineAsync("Headers:");
                await WriteHeadersAsync(writer, responseMessage.Headers);
                await writer.WriteLineAsync();
                if (responseMessage.Content != null)
                {
                    await writer.WriteLineAsync("Content headers:");
                    await WriteHeadersAsync(writer, responseMessage.Content.Headers);
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync("Content:");
                    await writer.WriteLineAsync(await responseMessage.Content.ReadAsStringAsync());
                }
            }
        }

        private async Task WriteHeadersAsync(StreamWriter writer, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            foreach (var header in headers)
            {
                var headerValue = header.Value.FirstOrDefault();
                await writer.WriteLineAsync($"{header.Key}: {headerValue}");
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
                line = await SkipEmptyLinesAsync(reader, line);
                if (line.StartsWith("Command:"))
                {
                    var splitPos = line.IndexOf(' ');
                    line = line.Substring(splitPos + 1);
                    splitPos = line.IndexOf(' ');
                    var expectedMethod = line.Substring(0, splitPos);
                    Assert.Equal(expectedMethod, requestMessage.Method.ToString());
                    var expectedCommand = line.Substring(splitPos + 1);
                    Assert.Equal(expectedCommand, requestMessage.RequestUri.AbsolutePath.Split('/').Last());
                    line = await reader.ReadLineAsync();
                }
                line = await SkipEmptyLinesAsync(reader, line);
                if (line.StartsWith("Headers:"))
                {
                    var expectedHeaders = new Dictionary<string, IEnumerable<string>>();
                    line = await ReadHeadersAsync(reader, expectedHeaders);
                    var actualHeaders = new Dictionary<string, IEnumerable<string>>();
                    foreach (var header in request.RequestMessage.Headers)
                        actualHeaders.Add(header.Key, header.Value);
                    ValidateHeaders(expectedHeaders, actualHeaders);
                }
                line = await SkipEmptyLinesAsync(reader, line);
                if (line.StartsWith("Content headers:"))
                {
                    var expectedHeaders = new Dictionary<string, IEnumerable<string>>();
                    line = await ReadHeadersAsync(reader, expectedHeaders);
                    var actualHeaders = new Dictionary<string, IEnumerable<string>>();
                    foreach (var header in request.RequestMessage.Content.Headers)
                        actualHeaders.Add(header.Key, header.Value);
                    ValidateHeaders(expectedHeaders, actualHeaders);
                }
                line = await SkipEmptyLinesAsync(reader, line);
                if (line.StartsWith("Content:"))
                {
                    var expectedContent = await reader.ReadToEndAsync();
                    var actualContent = await request.RequestMessage.Content.ReadAsStringAsync();
                    Assert.Equal(expectedContent, actualContent);
                }
            }
        }

        private void ValidateHeaders(
            IDictionary<string, IEnumerable<string>> expectedHeaders,
            IDictionary<string, IEnumerable<string>> actualHeaders)
        {
            Assert.Equal(expectedHeaders.Count(), actualHeaders.Count());
            foreach (var header in expectedHeaders)
            {
                Assert.Contains(header.Key, actualHeaders.Keys);
                Assert.Equal(header.Value.FirstOrDefault(), actualHeaders[header.Key].FirstOrDefault());
            }
        }

        private async Task<HttpResponseMessage> GetMockResponseAsync(string testMethodName, HttpRequestMessage requestMessage)
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
                line = await reader.ReadLineAsync();
                line = await SkipEmptyLinesAsync(reader, line);
                var statusCode = HttpStatusCode.OK;
                var headers = new Dictionary<string, IEnumerable<string>>();
                var contentHeaders = new Dictionary<string, IEnumerable<string>>();
                string content = null;
                while (line != null)
                {
                    if (line.StartsWith("Status:"))
                    {
                        statusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), line.Split(' ').Last());
                        line = await reader.ReadLineAsync();
                    }
                    line = await SkipEmptyLinesAsync(reader, line);
                    if (line.StartsWith("Headers:"))
                        line = await ReadHeadersAsync(reader, headers);
                    line = await SkipEmptyLinesAsync(reader, line);
                    if (line.StartsWith("Content headers:"))
                        line = await ReadHeadersAsync(reader, contentHeaders);
                    line = await SkipEmptyLinesAsync(reader, line);
                    if (line.StartsWith("Content:"))
                    {
                        content = await reader.ReadToEndAsync();
                        content = content.TrimEnd('\r', '\n');
                    }
                    var responseMessage = new HttpResponseMessage
                    {
                        StatusCode = statusCode,
                        Content = content == null 
                            ? null 
                            : new StreamContent(Utils.StringToStream(content)),
                        RequestMessage = requestMessage,
                        Version = new Version(1, 1),
                    };
                    foreach (var header in headers)
                    {
                        if (responseMessage.Headers.Contains(header.Key))
                            responseMessage.Headers.Remove(header.Key);
                        responseMessage.Headers.Add(header.Key, header.Value);
                    }
                    if (content != null)
                        foreach (var header in contentHeaders)
                        {
                            if (responseMessage.Content.Headers.Contains(header.Key))
                                responseMessage.Content.Headers.Remove(header.Key);
                            responseMessage.Content.Headers.Add(header.Key, header.Value);
                        }
                    return responseMessage;
                }
                return null;
            }
        }

        private async Task<string> ReadHeadersAsync(StreamReader reader, IDictionary<string, IEnumerable<string>> headers)
        {
            var line = await reader.ReadLineAsync();
            while (line != null && line.Length > 0)
            {
                var splitPos = line.IndexOf(':');
                var key = line.Substring(0, splitPos);
                var value = line.Substring(splitPos + 2);
                headers.Add(key, new[] { value });
                line = await reader.ReadLineAsync();
            }
            return line;
        }

        private async Task<string> SkipEmptyLinesAsync(StreamReader reader, string line)
        {
            while (line != null && line.Length == 0)
                line = await reader.ReadLineAsync();
            return line;
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
