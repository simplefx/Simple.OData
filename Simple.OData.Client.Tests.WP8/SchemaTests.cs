using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace Simple.OData.Client.Tests
{
    [TestClass]
    public class SchemaTests
    {
        [TestMethod]
        public void CheckODataOrgNorthwindSchema()
        {
            var client = new ODataClient("http://services.odata.org/V2/Northwind/Northwind.svc/");

            var table = client.Schema.FindTable("Product");
            Assert.AreEqual("ProductID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Categories");
            Assert.AreEqual("Categories", association.ReferenceTableName);
            Assert.AreEqual("0..1", association.Multiplicity);

            table = client.Schema.FindTable("Employees");
            association = table.FindAssociation("Employees");
            Assert.AreEqual("Employees", association.ReferenceTableName);
            Assert.AreEqual("0..1", association.Multiplicity);

            Assert.AreEqual(26, client.Schema.EntityTypes.Count());
            Assert.AreEqual(0, client.Schema.ComplexTypes.Count());
        }

        [TestMethod]
        public void CheckODataOrgODataSchema()
        {
            var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");

            var table = client.Schema.FindTable("Product");
            Assert.AreEqual("ID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Category_Products");
            Assert.AreEqual("Categories", association.ReferenceTableName);
            Assert.AreEqual("0..1", association.Multiplicity);

            var function = client.Schema.FindFunction("GetProductsByRating");
            Assert.AreEqual(RestVerbs.GET, function.HttpMethod);
            Assert.AreEqual("rating", function.Parameters[0]);

            Assert.AreEqual(3, client.Schema.EntityTypes.Count());
            Assert.AreEqual(1, client.Schema.ComplexTypes.Count());
            Assert.AreEqual(5, client.Schema.ComplexTypes.First().Properties.Count());
        }

        [TestMethod]
        public void CheckPluralsightComSchema()
        {
            var client = new ODataClient("http://pluralsight.com/odata/");

            var table = client.Schema.FindTable("Modules");
            Assert.AreEqual("Title", table.PrimaryKey[0]);

            Assert.IsNotNull(table.FindColumn("Author"));
            Assert.IsNotNull(table.FindColumn("Description"));

            var association = table.FindAssociation("Course");
            Assert.AreEqual("Courses", association.ReferenceTableName);
            Assert.AreEqual("*", association.Multiplicity);

            Assert.AreEqual(5, client.Schema.EntityTypes.Count());
            Assert.AreEqual(0, client.Schema.ComplexTypes.Count());
        }

        [TestMethod]
        public void RetrieveSchemaFromUrlWithoutFilename()
        {
            var client = new ODataClient("http://vancouverdataservice.cloudapp.net/v1/impark");

            var schema = client.Schema;

            Assert.IsTrue(schema.Tables.Any());
        }
    }
}
