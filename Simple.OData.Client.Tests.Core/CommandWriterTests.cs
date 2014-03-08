using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Xunit;
using Simple.OData.Client.Extensions;

namespace Simple.OData.Client.Tests
{
    public class CommandWriterTests : TestBase
    {
        private readonly ResponseReader _feedReader;
        private readonly CommandWriter _commandWriter;

        public CommandWriterTests()
        {
            _feedReader = new ResponseReader(_client.GetSchemaAsync().Result);
            _commandWriter = new CommandWriter(_client.GetSchemaAsync().Result);
        }

        [Fact]
        public void GetDataParsesSingleCustomerWithAddress()
        {
            string xml= GetResourceAsString("SingleCustomerWithAddress.xml");
            var document = XDocument.Parse(xml).Root;
            var row = _feedReader.GetData(xml).Single();
            var entry = _commandWriter.CreateEntry("Customers", row).Entry;

            AssertElementsCountEqual(document, entry, "m", "properties");
            AssertElementsContentEqual(document, entry, "d", "CustomerID");
            AssertElementsContentEqual(document, entry, "d", "CompanyName");
            AssertElementsContentEqual(document, entry, "d", "Address");
            AssertElementsContentEqual(document, entry, "d", "City");
            AssertElementsContentEqual(document, entry, "d", "Country");
        }

        private void AssertElementsCountEqual(XElement root1, XElement root2, string prefix, string name)
        {
            Assert.Equal(
                root1.Descendants(prefix, name).Elements().Count(),
                root2.Descendants(prefix, name).Elements().Count());
        }

        private void AssertElementsContentEqual(XElement root1, XElement root2, string prefix, string name)
        {
            var element1 = root1.Descendants(prefix, name).Single();
            var element2 = root2.Descendants(prefix, name).Single();
            Assert.Equal(element1.Value, element2.Value);
            Assert.Equal(element1.Attributes().Count(), element2.Attributes().Count());
            Assert.Equal(0, 
                element1.Attributes().Select(x => x.Value)
                .Except(element2.Attributes().Select(x => x.Value)).Count());
        }
    }
}