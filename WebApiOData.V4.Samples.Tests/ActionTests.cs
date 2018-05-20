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
    public class ActionTests
    {
        private readonly TestServer _server;
        private readonly ODataClient _client;

        public ActionTests()
        {
            _server = TestServer.Create<ActionStartup>();
            _client = new ODataClient(new ODataClientSettings()
            {
                BaseUri = new Uri("http://localhost/actions"),
                PayloadFormat = ODataPayloadFormat.Json,
                OnCreateMessageHandler = () => _server.Handler,
                OnTrace = (x,y) => Console.WriteLine(string.Format(x,y)),
            });
        }

        private void Dispose()
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
            var guid = new Guid("EC9DB412-AF7F-4157-B9F5-BFAB8A942B16");
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
            var guid = new Guid("E7857D7F-5B85-406A-A5C7-0DBA1D411576");
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