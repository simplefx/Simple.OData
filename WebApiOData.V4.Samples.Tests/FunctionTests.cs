using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Simple.OData.Client;
using WebApiOData.V4.Samples.Models;
using WebApiOData.V4.Samples.Startups;
using Xunit;

namespace WebApiOData.V4.Samples.Tests
{
    public class FunctionTests : IDisposable
    {
        private readonly TestServer _server;
        private readonly ODataClient _client;

        public FunctionTests()
        {
            _server = TestServer.Create<FunctionStartup>();
            _client = new ODataClient(new ODataClientSettings()
            {
                BaseUri = new Uri("http://localhost/functions"),
                PayloadFormat = ODataPayloadFormat.Json,
                OnCreateMessageHandler = () => _server.Handler,
                OnTrace = (x,y) => Console.WriteLine(string.Format(x,y)),
            });
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        [Fact]
        public async Task Get_the_most_expensive_product_untyped()
        {
            var result = (double)await _client
                .FindScalarAsync("Products/Default.MostExpensive()");

            Assert.InRange(result, 500, 1000);
        }

        [Fact]
        public async Task Get_the_most_expensive_product_untyped_batch()
        {
            object result = 0;
            var batch = new ODataBatch(_client);
            batch += async c => result = await c
                .FindScalarAsync("Products/Default.MostExpensive()");
            await batch.ExecuteAsync();

            Assert.InRange((double)result, 500, 1000);
        }

        [Fact]
        public async Task Get_the_most_expensive_product_typed()
        {
            var result = await _client
                .For<Product>()
                .Function("MostExpensive")
                .ExecuteAsScalarAsync<double>();

            Assert.InRange(result, 500, 1000);
        }

        [Fact]
        public async Task Get_the_most_expensive_product_dynamic()
        {
            var x = ODataDynamic.Expression;

            var result = await _client
                .For(x.Products)
                .Function("MostExpensive")
                .ExecuteAsScalarAsync<double>();

            Assert.InRange(result, 500, 1000);
        }

        [Fact]
        public async Task Get_the_top_10_expensive_products_untyped()
        {
            var result = await _client
                .FindEntriesAsync("Products/Default.Top10()");

            Assert.Equal(10, result.Count());
        }

        [Fact]
        public async Task Get_the_top_10_expensive_products_typed()
        {
            var result = await _client
                .For<Product>()
                .Function("Top10")
                .ExecuteAsEnumerableAsync();

            Assert.Equal(10, result.Count());
        }

        [Fact]
        public async Task Get_the_top_10_expensive_products_typed_array()
        {
            var result = await _client
                .For<Product>()
                .Function("Top10")
                .ExecuteAsArrayAsync<Product>();

            Assert.Equal(10, result.Count());
        }

        [Fact]
        public async Task Get_the_top_10_expensive_products_dynamic()
        {
            var x = ODataDynamic.Expression;

            IEnumerable<dynamic> result = await _client
                .For(x.Products)
                .Function("Top10")
                .ExecuteAsEnumerableAsync();

            Assert.Equal(10, result.Count());
        }

        [Fact]
        public async Task Get_the_rank_of_the_product_price_untyped()
        {
            var result = (int)await _client
                .FindScalarAsync("Products(33)/Default.GetPriceRank()");

            Assert.InRange(result, 0, 100);
        }

        [Fact]
        public async Task Get_the_rank_of_the_product_price_typed()
        {
            var result = await _client
                .For<Product>()
                .Key(33)
                .Function("GetPriceRank")
                .ExecuteAsScalarAsync<int>();

            Assert.InRange(result, 0, 100);
        }

        [Fact]
        public async Task Get_the_rank_of_the_product_price_dynamic()
        {
            var x = ODataDynamic.Expression;

            var result = await _client
                .For(x.Products)
                .Key(33)
                .Function("GetPriceRank")
                .ExecuteAsScalarAsync<int>();

            Assert.InRange(result, 0, 100);
        }

        [Fact]
        public async Task Get_the_sales_tax_untyped()
        {
            var result = (double)await _client
                .FindScalarAsync("Products(33)/Default.CalculateGeneralSalesTax(state='WA')");

            Assert.InRange(result, 1, 200);
        }

        [Fact]
        public async Task Get_the_sales_tax_typed()
        {
            var result = await _client
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
            var x = ODataDynamic.Expression;

            var result = await _client
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
            var result = await _client
                .ExecuteFunctionAsScalarAsync<double>("GetSalesTaxRate", 
                new Dictionary<string, object>() { { "state", "CA" } });

            Assert.InRange(result, 5, 20);
        }

        [Fact]
        public async Task Get_the_sales_tax_rate_typed()
        {
            var result = await _client
                .Unbound()
                .Function("GetSalesTaxRate")
                .Set(new { state = "CA" })
                .ExecuteAsScalarAsync<double>();

            Assert.InRange(result, 5, 20);
        }

        [Fact]
        public async Task Get_the_sales_tax_rate_dynamic()
        {
            var x = ODataDynamic.Expression;

            var result = await _client
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
