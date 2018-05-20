using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Xunit;
using Simple.OData.Client;
using Simple.OData.Client.TestUtils;
using WebApiOData.V3.Samples.Models;

namespace WebApiOData.V3.Samples.Tests
{
    public static partial class ODataClientSettingsExtensionMethods
    {
        private const string MockDataDir = @"../../../MockData";

        public static ODataClientSettings WithHttpMock(this ODataClientSettings settings)
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
            var baseType = typeof(ActionV3Tests);
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

    public class ActionV3Tests
    {
        private readonly TestServer _server;

        public ActionV3Tests()
        {
            _server = TestServer.Create<Startup>();
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        private ODataClientSettings CreateDefaultSettings()
        {
            return new ODataClientSettings()
            {
                BaseUri = new Uri("http://localhost/actions"),
                PayloadFormat = ODataPayloadFormat.Json,
                OnCreateMessageHandler = () => _server.Handler,
                OnTrace = (x, y) => Console.WriteLine(string.Format(x, y)),
            };
        }

        [Fact]
        public async Task Check_out_a_movie()
        {
            var settings = CreateDefaultSettings().WithHttpMock();
            var client = new ODataClient(settings);
            var isCheckedOut = false;
            Movie result = null;
            try
            {
                result = await client
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
                await client
                    .For<Movie>()
                    .Key(1)
                    .Action("Return")
                    .ExecuteAsSingleAsync();

                result = await client
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
            var settings = CreateDefaultSettings().WithHttpMock();
            var client = new ODataClient(settings);
            var result = await client
                .For<Movie>()
                .Key(1)
                .Action("Return")
                .ExecuteAsSingleAsync();

            Assert.Equal(1, result.ID);
        }

        [Fact]
        public async Task Check_out_several()
        {
            var settings = CreateDefaultSettings().WithHttpMock();
            var client = new ODataClient(settings);
            var result = await client
                .For<Movie>()
                .Action("CheckOutMany")
                .Set(new Dictionary<string, object>() { { "MovieIDs", new[] { 1, 2, 3 } } })
                .ExecuteAsEnumerableAsync();

            Assert.True(result.Count() > 1);
        }

        [Fact]
        public async Task CreateMovie()
        {
            var settings = CreateDefaultSettings().WithHttpMock();
            var client = new ODataClient(settings);
            var guid = new Guid("6B968CA9-4822-49EE-90BD-0439AAA48E9A");
            var result = await client
                .Unbound<Movie>()
                .Action("CreateMovie")
                .Set(new { Title = guid.ToString() })
                .ExecuteAsSingleAsync();

            Assert.True(result.ID > 0);
        }

        [Fact]
        public async Task CreateMovie_batch()
        {
            var settings = CreateDefaultSettings().WithHttpMock();
            var client = new ODataClient(settings);
            var guid = new Guid("2C44053F-6790-4221-934E-BA214DFEB643");
            Movie result = null;
            var batch = new ODataBatch(client);
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