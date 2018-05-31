using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

using Xunit;
#if NET452 && !MOCK_HTTP
using Simple.OData.NorthwindModel;
#endif

namespace Simple.OData.Client.Tests
{
    public class TestBase : IDisposable
    {
        protected Uri _serviceUri;
#if NET452 && !MOCK_HTTP
        protected TestService _service;
#endif
        protected IODataClient _client;
        protected readonly bool _readOnlyTests;

        protected TestBase(bool readOnlyTests = false)
        {
            _readOnlyTests = readOnlyTests;
#if NET452 && !MOCK_HTTP
            _service = new TestService(typeof(NorthwindService));
            _serviceUri = _service.ServiceUri;
#else
            _serviceUri = new Uri("http://localhost/");
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
#if NET452 && !MOCK_HTTP
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

        private string GetMetadataDocument()
        {
#if MOCK_HTTP
            return MetadataResolver.GetMetadataDocument("Northwind.xml");
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
