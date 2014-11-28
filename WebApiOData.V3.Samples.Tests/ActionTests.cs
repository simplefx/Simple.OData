using System;
using System.Collections.Generic;
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
                UrlBase = "http://localhost/actions",
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
            await _client
                .For("Movies")
                .Key(1)
                .Action("CheckOut")
                .ExecuteAsync();
        }

        [Fact]
        public async Task Return_a_movie()
        {
            await _client
                .For("Movies")
                .Key(1)
                .Action("Return")
                .ExecuteAsync();
        }

        [Fact]
        public async Task Check_out_several()
        {
            await _client
                .For("Movies")
                .Action("CheckOutMany")
                .Set(new Dictionary<string, object>() { { "MovieIDs", new[] { 1, 2, 3 } } })
                .ExecuteAsync();
        }

        [Fact]
        public async Task CreateMovie()
        {
            await _client
                .Unbound()
                .Action("CreateMovie")
                .Set(new { Title = Guid.NewGuid().ToString() })
                .ExecuteAsync();
        }
    }
}