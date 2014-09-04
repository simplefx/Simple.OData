using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Data.OData;
using Moq;

#pragma warning disable 3008

namespace Simple.OData.Client.Tests
{
    public class TestBase : IDisposable
    {
        protected IODataClient _client;

        public TestBase() 
            : this("Northwind.edmx")
        {
        }

        public TestBase(string metadataFile)
        {
            if (!string.IsNullOrEmpty(metadataFile))
            {
                _client = CreateClient(metadataFile);
            }
        }

        public IODataClient CreateClient(string metadataFile)
        {
            var urlBase = "http://localhost/" + metadataFile;
            var metadataString = GetResourceAsString(@"Resources." + metadataFile);
            Session.Add(urlBase, Session.FromMetadata(urlBase, metadataString));
            return new ODataClient(urlBase);
        }

        public void Dispose()
        {
        }

        public static string GetResourceAsString(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();
            string completeResourceName = resourceNames.FirstOrDefault(o => o.EndsWith("." + resourceName, StringComparison.CurrentCultureIgnoreCase));
            using (Stream resourceStream = assembly.GetManifestResourceStream(completeResourceName))
            {
                TextReader reader = new StreamReader(resourceStream);
                return reader.ReadToEnd();
            }
        }

        public IODataResponseMessageAsync SetUpResourceMock(string resourceName)
        {
            var document = GetResourceAsString(resourceName);
            var mock = new Mock<IODataResponseMessageAsync>();
            mock.Setup(x => x.GetStreamAsync()).ReturnsAsync(new MemoryStream(Encoding.UTF8.GetBytes(document)));
            mock.Setup(x => x.GetStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes(document)));
            mock.Setup(x => x.GetHeader("Content-Type")).Returns(() => "application/atom+xml; type=feed; charset=utf-8");
            var response = mock.Object;
            return response;
        }
    }
}
