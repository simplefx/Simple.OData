using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Simple.OData.Client.Tests
{
    public class TestBase : IDisposable
    {
        protected IODataClient _client;

        public TestBase()
        {
            _client = CreateClientWithDefaultSettings();
        }

        public IODataClient CreateClientWithDefaultSettings()
        {
            var urlBase = "http://localhost/FakeClient";
            var schemaString = GetResourceAsString(@"Resources.Northwind.edmx");
            Schema.Add(urlBase, ODataClient.ParseSchemaString(schemaString));
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
    }
}
