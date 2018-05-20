using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Simple.OData.Client;
using Simple.OData.Client.TestUtils;
using WebApiOData.V4.Samples.Models;
using WebApiOData.V4.Samples.Startups;
using Xunit;

namespace WebApiOData.V4.Samples.Tests
{
    public static partial class ODataClientSettingsExtensionMethods2
    {
        private const string MockDataDir = @"../../../MockData";

        public static ODataClientSettings WithHttpMock2(this ODataClientSettings settings)
        {
            var methodName = GetTestMethodFullName();
            var mockDataPathBase = GetMockDataPathBase(methodName);
#if MOCK_HTTP
            var recording = false;
#else
            var recording = true;
#endif
            var requestExecutor = new MockingRequestExecutor(settings, mockDataPathBase, recording);
            settings.RequestExecutor = requestExecutor.ExecuteRequestAsync;
            return settings;
        }

        private static string GetMockDataPathBase(string testMethodName)
        {
            return Path.Combine(MockDataDir, testMethodName);
        }

        private static string GetTestMethodFullName()
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            var baseType = typeof(FunctionV4Tests);
            for (var frameNumber = 1; ; frameNumber++)
            {
                var stackFrame = stackTrace.GetFrame(frameNumber);
                if (stackFrame == null)
                    throw new InvalidOperationException("Attempt to retrieve a frame beyond the call stack");
                var method = stackFrame.GetMethod();
                var methodName = new string(method.Name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
                if (method.DeclaringType == baseType)
                    return string.Format($"{method.DeclaringType.Name}.{methodName}");
            }
        }
    }

    public class FunctionV4Tests : IDisposable
    {
        private readonly TestServer _server;

        public FunctionV4Tests()
        {
            _server = TestServer.Create<FunctionStartup>();
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        private ODataClientSettings CreateDefaultSettings()
        {
            return new ODataClientSettings()
            {
                BaseUri = new Uri("http://localhost/functions"),
                PayloadFormat = ODataPayloadFormat.Json,
                OnCreateMessageHandler = () => _server.Handler,
                OnTrace = (x, y) => Console.WriteLine(string.Format(x, y)),
            };
        }

        [Fact]
        public async Task Get_the_most_expensive_product_untyped()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var result = (double)await client
                .FindScalarAsync("Products/Default.MostExpensive()");

            Assert.InRange(result, 500, 1000);
        }

        [Fact]
        public async Task Get_the_most_expensive_product_untyped_batch()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            object result = 0;
            var batch = new ODataBatch(settings);
            batch += async c => result = await c
                .FindScalarAsync("Products/Default.MostExpensive()");
            await batch.ExecuteAsync();

            Assert.InRange((double)result, 500, 1000);
        }

        [Fact]
        public async Task Get_the_most_expensive_product_typed()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var result = await client
                .For<Product>()
                .Function("MostExpensive")
                .ExecuteAsScalarAsync<double>();

            Assert.InRange(result, 500, 1000);
        }

        [Fact]
        public async Task Get_the_most_expensive_product_dynamic()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var x = ODataDynamic.Expression;

            var result = await client
                .For(x.Products)
                .Function("MostExpensive")
                .ExecuteAsScalarAsync<double>();

            Assert.InRange(result, 500, 1000);
        }

        [Fact]
        public async Task Get_the_top_10_expensive_products_untyped()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var result = await client
                .FindEntriesAsync("Products/Default.Top10()");

            Assert.Equal(10, result.Count());
        }

        [Fact]
        public async Task Get_the_top_10_expensive_products_typed()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var result = await client
                .For<Product>()
                .Function("Top10")
                .ExecuteAsEnumerableAsync();

            Assert.Equal(10, result.Count());
        }

        [Fact]
        public async Task Get_the_top_10_expensive_products_typed_array()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var result = await client
                .For<Product>()
                .Function("Top10")
                .ExecuteAsArrayAsync<Product>();

            Assert.Equal(10, result.Count());
        }

        [Fact]
        public async Task Get_the_top_10_expensive_products_dynamic()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var x = ODataDynamic.Expression;

            IEnumerable<dynamic> result = await client
                .For(x.Products)
                .Function("Top10")
                .ExecuteAsEnumerableAsync();

            Assert.Equal(10, result.Count());
        }

        [Fact]
        public async Task Get_the_rank_of_the_product_price_untyped()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var result = (int)await client
                .FindScalarAsync("Products(33)/Default.GetPriceRank()");

            Assert.InRange(result, 0, 100);
        }

        [Fact]
        public async Task Get_the_rank_of_the_product_price_typed()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var result = await client
                .For<Product>()
                .Key(33)
                .Function("GetPriceRank")
                .ExecuteAsScalarAsync<int>();

            Assert.InRange(result, 0, 100);
        }

        [Fact]
        public async Task Get_the_rank_of_the_product_price_dynamic()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var x = ODataDynamic.Expression;

            var result = await client
                .For(x.Products)
                .Key(33)
                .Function("GetPriceRank")
                .ExecuteAsScalarAsync<int>();

            Assert.InRange(result, 0, 100);
        }

        [Fact]
        public async Task Get_the_sales_tax_untyped()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var result = (double)await client
                .FindScalarAsync("Products(33)/Default.CalculateGeneralSalesTax(state='WA')");

            Assert.InRange(result, 1, 200);
        }

        [Fact]
        public async Task Get_the_sales_tax_typed()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var result = await client
                .For<Product>()
                .Key(33)
                .Function("CalculateGeneralSalesTax")
                .Set(new { state = "WA" })
                .ExecuteAsScalarAsync<double>();

            Assert.InRange(result, 1, 200);
        }

        [Fact]
        public async Task Get_the_sales_tax_dynamic()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var x = ODataDynamic.Expression;

            var result = await client
                .For(x.Products)
                .Key(33)
                .Function("CalculateGeneralSalesTax")
                .Set(x.state = "WA")
                .ExecuteAsScalarAsync<double>();

            Assert.InRange(result, 1, 200);
        }

        [Fact]
        public async Task Get_the_sales_tax_rate_untyped()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var result = await client
                .ExecuteFunctionAsScalarAsync<double>("GetSalesTaxRate", 
                new Dictionary<string, object>() { { "state", "CA" } });

            Assert.InRange(result, 5, 20);
        }

        [Fact]
        public async Task Get_the_sales_tax_rate_typed()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var result = await client
                .Unbound()
                .Function("GetSalesTaxRate")
                .Set(new { state = "CA" })
                .ExecuteAsScalarAsync<double>();

            Assert.InRange(result, 5, 20);
        }

        [Fact]
        public async Task Get_the_sales_tax_rate_dynamic()
        {
            var settings = CreateDefaultSettings().WithHttpMock2();
            var client = new ODataClient(settings);
            var x = ODataDynamic.Expression;

            var result = await client
                .Unbound()
                .Function("GetSalesTaxRate")
                .Set(x.state = "CA")
                .ExecuteAsScalarAsync<double>();

            Assert.InRange(result, 5, 20);
        }

        //[Fact]
        //public async Task Call_function_with_parameter_alias_untyped()
        //{
        //    var result = await _client
        //        .ExecuteFunctionAsScalarAsync("GetSalesTaxRate(state=@p1)?@p1='ND'");

        //    Assert.InRange(result, 5, 20);
        //}
    }
}
