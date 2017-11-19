using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Simple.OData.Client.TestUtils
{
    public class MockingRequestExecutor
    {
        private readonly ODataClientSettings _settings;
        private readonly string _mockDataPath;
        private readonly bool _recording;

        public MockingRequestExecutor(ODataClientSettings settings, string mockDataPath, bool recording = false)
        {
            _settings = settings;
            _mockDataPath = mockDataPath;
            _recording = recording;
        }

        public async Task<HttpResponseMessage> ExecuteRequestAsync(HttpRequestMessage request)
        {
            if (_recording)
                await SaveRequestAsync(request);
            else
                await ValidateRequestAsync(request);

            if (_recording)
            {
                var httpConnection = new HttpConnection(_settings);
                var response = await httpConnection.HttpClient.SendAsync(request);
                await SaveResponseAsync(response);
                return response;
            }
            else
            {
                return await GetMockResponseAsync(request);
            }
        }

        private async Task SaveRequestAsync(HttpRequestMessage request)
        {
            using (var writer = new StreamWriter(_mockDataPath, false))
            {
                await writer.WriteLineAsync($"--- Request ---");
                await writer.WriteLineAsync();
                var methodName = request.Method.ToString();
                var commandText = request.RequestUri.AbsolutePath.Split('/').Last();
                await writer.WriteLineAsync($"Command: {methodName} {commandText}");
                await writer.WriteLineAsync();
                await writer.WriteLineAsync("Headers:");
                await WriteHeadersAsync(writer, request.Headers);
                await writer.WriteLineAsync();
                if (request.Content != null)
                {
                    await writer.WriteLineAsync("Content headers:");
                    await WriteHeadersAsync(writer, request.Content.Headers);
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync("Content:");
                    await writer.WriteLineAsync(await request.Content.ReadAsStringAsync());
                }
            }
        }

        private async Task SaveResponseAsync(HttpResponseMessage responseMessage)
        {
            using (var writer = new StreamWriter(_mockDataPath, true))
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

        private async Task ValidateRequestAsync(HttpRequestMessage request)
        {
            using (var reader = new StreamReader(_mockDataPath))
            {
                var line = await reader.ReadLineAsync();
                while (line != "--- Request ---")
                {
                    Assert.NotNull(line);
                    line = await reader.ReadLineAsync();
                }
                line = await reader.ReadLineAsync();
                line = await SkipEmptyLinesAsync(reader, line);
                if (line.StartsWith("Command:"))
                {
                    var splitPos = line.IndexOf(' ');
                    line = line.Substring(splitPos + 1);
                    splitPos = line.IndexOf(' ');
                    var expectedMethod = line.Substring(0, splitPos);
                    var actualMethod = request.Method.ToString();
                    Assert.Equal(expectedMethod, actualMethod);
                    var expectedCommand = line.Substring(splitPos + 1);
                    var actualCommand = request.RequestUri.AbsolutePath.Split('/').Last();
                    Assert.Equal(expectedCommand, actualCommand);
                    line = await reader.ReadLineAsync();
                }
                line = await SkipEmptyLinesAsync(reader, line);
                if (line.StartsWith("Headers:"))
                {
                    var expectedHeaders = new Dictionary<string, IEnumerable<string>>();
                    line = await ReadHeadersAsync(reader, expectedHeaders);
                    var actualHeaders = new Dictionary<string, IEnumerable<string>>();
                    foreach (var header in request.Headers)
                        actualHeaders.Add(header.Key, header.Value);
                    ValidateHeaders(expectedHeaders, actualHeaders);
                }
                line = await SkipEmptyLinesAsync(reader, line);
                if (line.StartsWith("Content headers:"))
                {
                    var expectedHeaders = new Dictionary<string, IEnumerable<string>>();
                    line = await ReadHeadersAsync(reader, expectedHeaders);
                    var actualHeaders = new Dictionary<string, IEnumerable<string>>();
                    foreach (var header in request.Content.Headers)
                        actualHeaders.Add(header.Key, header.Value);
                    ValidateHeaders(expectedHeaders, actualHeaders);
                }
                line = await SkipEmptyLinesAsync(reader, line);
                if (line.StartsWith("Content:"))
                {
                    var expectedContent = await reader.ReadToEndAsync();
                    var pos = expectedContent.IndexOf("--- Response ---");
                    if (pos > 0)
                        expectedContent = expectedContent.Substring(0, pos).TrimEnd('\r', '\n');
                    expectedContent = RemoveElements(expectedContent, new[] { "updated" });
                    var actualContent = RemoveElements(await request.Content.ReadAsStringAsync(), new[] { "updated" });
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

        private async Task<HttpResponseMessage> GetMockResponseAsync(HttpRequestMessage request)
        {
            using (var reader = new StreamReader(_mockDataPath))
            {
                var line = await reader.ReadLineAsync();
                while (line != "--- Response ---")
                {
                    Assert.NotNull(line);
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
                    var response = new HttpResponseMessage
                    {
                        StatusCode = statusCode,
                        Content = content == null
                            ? null
                            : new StreamContent(Utils.StringToStream(content)),
                        RequestMessage = request,
                        Version = new Version(1, 1),
                    };
                    foreach (var header in headers)
                    {
                        if (response.Headers.Contains(header.Key))
                            response.Headers.Remove(header.Key);
                        response.Headers.Add(header.Key, header.Value);
                    }
                    if (content != null)
                        foreach (var header in contentHeaders)
                        {
                            if (response.Content.Headers.Contains(header.Key))
                                response.Content.Headers.Remove(header.Key);
                            response.Content.Headers.Add(header.Key, header.Value);
                        }
                    return response;
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

        private string RemoveElements(string content, IEnumerable<string> elementNames)
        {
            foreach (var elementName in elementNames)
            {
                while (true)
                {
                    var startPos = content.IndexOf($"<{elementName}>");
                    var endPos = content.IndexOf($"</{elementName}>");
                    if (startPos >= 0 && endPos > startPos)
                    {
                        content = content.Substring(0, startPos) + content.Substring(endPos + elementName.Length + 3);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return content;
        }
    }
}
