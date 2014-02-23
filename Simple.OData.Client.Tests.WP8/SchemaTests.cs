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

            var table = client.GetSchema().FindTable("Product");
            Assert.AreEqual("ProductID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Categories");
            Assert.AreEqual("Categories", association.ReferenceTableName);
            Assert.AreEqual("0..1", association.Multiplicity);

            table = client.GetSchema().FindTable("Employees");
            association = table.FindAssociation("Employees");
            Assert.AreEqual("Employees", association.ReferenceTableName);
            Assert.AreEqual("0..1", association.Multiplicity);

            Assert.AreEqual(26, client.GetSchema().EntityTypes.Count());
            Assert.AreEqual(0, client.GetSchema().ComplexTypes.Count());
        }

        [TestMethod]
        public void CheckODataOrgODataV2Schema()
        {
            var client = new ODataClient("http://services.odata.org/V2/OData/OData.svc/");

            var table = client.GetSchema().FindTable("Product");
            Assert.AreEqual("ID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Category_Products");
            Assert.AreEqual("Categories", association.ReferenceTableName);
            Assert.AreEqual("0..1", association.Multiplicity);

            var function = client.GetSchema().FindFunction("GetProductsByRating");
            Assert.AreEqual(RestVerbs.GET, function.HttpMethod);
            Assert.AreEqual("rating", function.Parameters[0]);

            Assert.AreEqual(3, client.GetSchema().EntityTypes.Count());
            Assert.AreEqual(1, client.GetSchema().ComplexTypes.Count());
            Assert.AreEqual(5, client.GetSchema().ComplexTypes.First().Properties.Count());
        }

        [TestMethod]
        public void CheckODataOrgODataV3Schema()
        {
            var client = new ODataClient("http://services.odata.org/V3/OData/OData.svc/");

            var table = client.GetSchema().FindTable("Product");
            Assert.AreEqual("ID", table.PrimaryKey[0]);

            var association = table.FindAssociation("Category_Products");
            Assert.AreEqual("Categories", association.ReferenceTableName);
            Assert.AreEqual("*", association.Multiplicity);

            var function = client.GetSchema().FindFunction("GetProductsByRating");
            Assert.AreEqual(RestVerbs.GET, function.HttpMethod);
            Assert.AreEqual("rating", function.Parameters[0]);

            Assert.AreEqual(10, client.GetSchema().EntityTypes.Count());
            Assert.AreEqual(1, client.GetSchema().ComplexTypes.Count());
            Assert.AreEqual(5, client.GetSchema().ComplexTypes.First().Properties.Count());
        }
    }
}
