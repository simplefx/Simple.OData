using System;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Simple.OData.Client;
using WebApiOData.V4.Samples.Models;
using WebApiOData.V4.Samples.Startups;
using Xunit;

namespace WebApiOData.V4.Samples.Tests
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
                UrlBase = "http://localhost/actions",
                OnCreateMessageHandler = () => _server.Handler,
                OnTrace = (x,y) => Console.WriteLine(string.Format(x,y)),
            });
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        [Fact]
        public async Task Test()
        {
            var result = await _client.For<Movie>().FindEntriesAsync();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Check_out_a_movie_untyped()
        {
            var result = await _client
                .For("Movies")
                .Key(1)
                .Action("CheckOut")
                .ExecuteAsync();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Return_a_movie_untyped()
        {
            var result = await _client
                .For("Movies")
                .Key(1)
                .Action("Return")
                .ExecuteAsync();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Check_out_several_untyped()
        {
            var result = await _client
                .For("Movies")
                .Action("CheckOutMany")
                .Set(new [] {1, 2, 3})
                .ExecuteAsync();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateMovie()
        {
            var result = await _client
                .ExecuteActionAsync("CreateMovie", null);

            Assert.NotNull(result);
        }
    }
}