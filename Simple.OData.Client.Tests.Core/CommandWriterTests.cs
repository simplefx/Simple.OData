using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Xunit;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client.Tests
{
    public class CommandWriterTests : TestBase
    {
        [Fact]
        public void CreateEntryCustomerWithAddress()
        {
            var feedReader = new ResponseReader(_client.GetSchemaAsync().Result);
            var commandWriter = new CommandWriter(_client.GetSchemaAsync().Result);

            string xml = GetResourceAsString("SingleCustomerWithAddress.xml");
            var document = XDocument.Parse(xml).Root;
            var row = feedReader.GetData(xml).Single();
            var entry = commandWriter.CreateEntry("Customers", row).Entry;

            AssertElementsCountEqual(document, entry, "m", "properties");
            AssertElementsContentEqual(document, entry, "d", "CustomerID");
            AssertElementsContentEqual(document, entry, "d", "CompanyName");
            AssertElementsContentEqual(document, entry, "d", "Address");
            AssertElementsContentEqual(document, entry, "d", "Type");
            AssertElementsContentEqual(document, entry, "d", "City1");
            AssertElementsContentEqual(document, entry, "d", "Region");
            AssertElementsContentEqual(document, entry, "d", "PostalCode");
            AssertElementsContentEqual(document, entry, "d", "Country");
        }

        [Fact]
        public void CreateEntryWorkTaskModel()
        {
            var client = CreateClient("QAS.Multiplatform.Demo.edmx");
            var feedReader = new ResponseReader(client.GetSchemaAsync().Result);
            var commandWriter = new CommandWriter(client.GetSchemaAsync().Result);

            string xml = GetResourceAsString("WorkTaskModel.xml");
            var document = XDocument.Parse(xml).Root;
            var row = feedReader.GetData(xml).Single();
            var entry = commandWriter.CreateEntry("WorkTaskModel", row).Entry;

            AssertElementsCountEqual(document, entry, "m", "properties");
            AssertElementsContentEqual(document, entry, "d", "Id");
            AssertElementsContentEqual(document, entry, "d", "Code");
            AssertElementsContentEqual(document, entry, "d", "StartDate", false);
            AssertElementsContentEqual(document, entry, "d", "EndDate", false);
            AssertElementsContentEqual(document, entry, "d", "State");
            AssertElementsContentEqual(document, entry, "d", "Location", false);
            AssertElementsContentEqual(document, entry, "d", "Latitude", false);
            AssertElementsContentEqual(document, entry, "d", "Longitude", false);
            AssertElementsContentEqual(document, entry, "d", "WorkerId");
            AssertElementsContentEqual(document, entry, "d", "CustomerId");
        }

        [Fact]
        public void CreateEntryWorkTaskModelWithNulls()
        {
            var client = CreateClient("QAS.Multiplatform.Demo.edmx");
            var feedReader = new ResponseReader(client.GetSchemaAsync().Result);
            var commandWriter = new CommandWriter(client.GetSchemaAsync().Result);

            string xml = GetResourceAsString("WorkTaskModelWithNulls.xml");
            var document = XDocument.Parse(xml).Root;
            var row = feedReader.GetData(xml).Single();
            var entry = commandWriter.CreateEntry("WorkTaskModel", row).Entry;

            AssertElementsCountEqual(document, entry, "m", "properties");
            AssertElementsContentEqual(document, entry, "d", "Id");
            AssertElementsContentEqual(document, entry, "d", "StartDate", false);
            AssertElementsContentEqual(document, entry, "d", "EndDate", false);
            AssertElementsContentEqual(document, entry, "d", "State");
            AssertElementsContentEqual(document, entry, "d", "WorkerId");
            AssertElementsContentEqual(document, entry, "d", "CustomerId");
        }

        private void AssertElementsCountEqual(XElement root1, XElement root2, string prefix, string name)
        {
            Assert.Equal(
                root1.Descendants(prefix, name).Elements().Count(),
                root2.Descendants(prefix, name).Elements().Count());
        }

        private void AssertElementsContentEqual(XElement root1, XElement root2, string prefix, string name, bool compareValues = true)
        {
            if (!root1.Descendants(prefix, name).Any() && !root2.Descendants(prefix, name).Any())
                return;

            var element1 = root1.Descendants(prefix, name).Single();
            var element2 = root2.Descendants(prefix, name).Single();
            Assert.Equal(element1.Attributes().Count(), element2.Attributes().Count());
            Assert.Equal(0, 
                element1.Attributes().Select(x => x.Value)
                .Except(element2.Attributes().Select(x => x.Value)).Count());
            if (compareValues)
            {
                Assert.Equal(element1.Value, element2.Value, StringComparer.InvariantCultureIgnoreCase);
            }
            else
            {
                if (element1.Value == null)
                    Assert.Null(element2.Value);
                if (element1.Value == string.Empty)
                    Assert.Equal(string.Empty, element2.Value);
            }
        }
    }
}