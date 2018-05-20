using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Simple.OData.Client;
using WebApiOData.V3.Samples.Models;
using Xunit;

namespace WebApiOData.V3.Samples.Tests
{
    public class ActionTests
    {
        private readonly TestServer _server;
        private readonly ODataClient _client;

        public ActionTests()
        {
            _server = TestServer.Create<Startup>();
            _client = new ODataClient(new ODataClientSettings()
            {
                BaseUri = new Uri("http://localhost/actions"),
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
        public async Task Check_out_a_movie()
        {
            var isCheckedOut = false;
            Movie result = null;
            try
            {
                result = await _client
                    .For<Movie>()
                    .Key(1)
                    .Action("CheckOut")
                    .ExecuteAsSingleAsync();
            }
            catch (WebRequestException)
            {
                isCheckedOut = true;
            }

            if (isCheckedOut)
            {
                await _client
                    .For<Movie>()
                    .Key(1)
                    .Action("Return")
                    .ExecuteAsSingleAsync();

                result = await _client
                    .For<Movie>()
                    .Key(1)
                    .Action("CheckOut")
                    .ExecuteAsSingleAsync();
            }

            Assert.Equal(1, result.ID);
        }

        [Fact]
        public async Task Return_a_movie()
        {
            var result = await _client
                .For<Movie>()
                .Key(1)
                .Action("Return")
                .ExecuteAsSingleAsync();

            Assert.Equal(1, result.ID);
        }

        [Fact]
        public async Task Check_out_several()
        {
            var result = await _client
                .For<Movie>()
                .Action("CheckOutMany")
                .Set(new Dictionary<string, object>() { { "MovieIDs", new[] { 1, 2, 3 } } })
                .ExecuteAsEnumerableAsync();

            Assert.True(result.Count() > 1);
        }

        [Fact]
        public async Task CreateMovie()
        {
            var guid = new Guid("6B968CA9-4822-49EE-90BD-0439AAA48E9A");
            var result = await _client
                .Unbound<Movie>()
                .Action("CreateMovie")
                .Set(new { Title = guid.ToString() })
                .ExecuteAsSingleAsync();

            Assert.True(result.ID > 0);
        }

        [Fact]
        public async Task CreateMovie_batch()
        {
            var guid = new Guid("2C44053F-6790-4221-934E-BA214DFEB643");
            Movie result = null;
            var batch = new ODataBatch(_client);
            batch += async c => result = await c
                .Unbound<Movie>()
                .Action("CreateMovie")
                .Set(new { Title = guid.ToString() })
                .ExecuteAsSingleAsync();
            await batch.ExecuteAsync();

            Assert.True(result.ID > 0);
        }
    }
}